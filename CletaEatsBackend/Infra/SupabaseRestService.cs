using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CletaEatsBackend.Modelo;
using Microsoft.Extensions.Options;

namespace CletaEatsBackend.Infra;

/// <summary>
/// Acceso a tablas Supabase vía PostgREST (misma forma que el cliente JS anterior).
/// </summary>
public class SupabaseRestService
{
    private readonly HttpClient _http;
    private readonly SupabaseConnectionOptions _opt;
    private static readonly JsonSerializerOptions SnakeOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public SupabaseRestService(HttpClient http, IOptions<SupabaseConnectionOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
        var root = _opt.Url.TrimEnd('/') + "/";
        if (_http.BaseAddress == null || _http.BaseAddress.ToString() != root)
            _http.BaseAddress = new Uri(root);
    }

    private string ApiKey => _opt.RestApiKey;

    private void ApplyAuth(HttpRequestMessage req)
    {
        req.Headers.TryAddWithoutValidation("apikey", ApiKey);
        req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {ApiKey}");
    }

    private async Task<(bool Ok, string Body, HttpStatusCode Code)> SendAsync(HttpRequestMessage req)
    {
        ApplyAuth(req);
        using var res = await _http.SendAsync(req);
        var body = await res.Content.ReadAsStringAsync();
        return (res.IsSuccessStatusCode, body, res.StatusCode);
    }

    private static string Err(string body, HttpStatusCode code) =>
        string.IsNullOrWhiteSpace(body) ? $"Error HTTP {(int)code}" : body.Trim()[..Math.Min(body.Length, 400)];

    private async Task<JsonDocument?> GetJsonDoc(string relativeUri)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, relativeUri);
        var (ok, body, code) = await SendAsync(req);
        if (!ok) throw new InvalidOperationException(Err(body, code));
        return string.IsNullOrWhiteSpace(body) ? null : JsonDocument.Parse(body);
    }

    private async Task<JsonElement?> PostReturningRow(string relativeUri, object payload)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, relativeUri);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        req.Headers.TryAddWithoutValidation("Prefer", "return=representation");
        req.Content = new StringContent(JsonSerializer.Serialize(payload, SnakeOpts), Encoding.UTF8, "application/json");
        var (ok, body, code) = await SendAsync(req);
        if (!ok) throw new InvalidOperationException(Err(body, code));
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            return root[0];
        if (root.ValueKind == JsonValueKind.Object)
            return root;
        return null;
    }

    private async Task PostNoBodyExpectOk(string relativeUri, object payload)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, relativeUri);
        req.Content = new StringContent(JsonSerializer.Serialize(payload, SnakeOpts), Encoding.UTF8, "application/json");
        var (ok, body, code) = await SendAsync(req);
        if (!ok) throw new InvalidOperationException(Err(body, code));
    }

    private async Task PatchExpectOk(string relativeUri, object payload)
    {
        using var req = new HttpRequestMessage(HttpMethod.Patch, relativeUri);
        req.Content = new StringContent(JsonSerializer.Serialize(payload, SnakeOpts), Encoding.UTF8, "application/json");
        var (ok, body, code) = await SendAsync(req);
        if (!ok) throw new InvalidOperationException(Err(body, code));
    }

    private async Task DeleteExpectOk(string relativeUri)
    {
        using var req = new HttpRequestMessage(HttpMethod.Delete, relativeUri);
        var (ok, body, code) = await SendAsync(req);
        if (!ok) throw new InvalidOperationException(Err(body, code));
    }

    private static TipoComida ParseTipo(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return TipoComida.OTRA;
        return Enum.TryParse<TipoComida>(s.Trim(), true, out var t) ? t : TipoComida.OTRA;
    }

    public async Task<List<Restaurante>> ObtenerRestaurantesAsync()
    {
        using var doc = await GetJsonDoc("rest/v1/restaurantes?select=*&order=nombre.asc") ?? throw new InvalidOperationException("Sin datos.");
        var list = new List<Restaurante>();
        foreach (var row in doc.RootElement.EnumerateArray())
        {
            list.Add(new Restaurante(
                row.GetProperty("id").GetInt32(),
                row.GetProperty("nombre").GetString() ?? "",
                row.GetProperty("cedula_juridica").GetString() ?? "",
                row.GetProperty("direccion").GetString() ?? "",
                ParseTipo(row.GetProperty("tipo_comida").GetString())));
        }
        return list;
    }

    public async Task<string> RegistrarRestauranteAsync(string nombre, string cedulaJuridica, string direccion, string tipoComidaStr)
    {
        if (!Enum.TryParse<TipoComida>(tipoComidaStr, true, out var tipo))
            return "Tipo de comida no valido. Use: RAPIDA, CHINA, SALUDABLE, ITALIANA, MEXICANA, MARISCOS, OTRA";
        foreach (var r in await ObtenerRestaurantesAsync())
            if (string.Equals(r.CedulaJuridica, cedulaJuridica, StringComparison.OrdinalIgnoreCase))
                return $"Error: cedula juridica {cedulaJuridica} ya registrada.";
        await PostNoBodyExpectOk("rest/v1/restaurantes", new[]
        {
            new Dictionary<string, object?>
            {
                ["nombre"] = nombre.Trim(),
                ["cedula_juridica"] = cedulaJuridica.Trim(),
                ["direccion"] = direccion.Trim(),
                ["tipo_comida"] = tipo.ToString(),
            },
        });
        return $"Restaurante '{nombre.Trim()}' registrado.";
    }

    public async Task<string> ActualizarRestauranteAsync(int id, string nombre, string cedulaJuridica, string direccion, string tipoComidaStr)
    {
        var todos = await ObtenerRestaurantesAsync();
        if (todos.All(r => r.Id != id))
            return "Error: restaurante no encontrado.";
        if (!Enum.TryParse<TipoComida>(tipoComidaStr, true, out var tipo))
            return "Tipo de comida no valido. Use: RAPIDA, CHINA, SALUDABLE, ITALIANA, MEXICANA, MARISCOS, OTRA";
        var otro = todos.FirstOrDefault(r => string.Equals(r.CedulaJuridica, cedulaJuridica, StringComparison.OrdinalIgnoreCase));
        if (otro != null && otro.Id != id)
            return $"Error: cedula juridica {cedulaJuridica} ya registrada.";
        await PatchExpectOk($"rest/v1/restaurantes?id=eq.{id}", new Dictionary<string, object?>
        {
            ["nombre"] = nombre.Trim(),
            ["cedula_juridica"] = cedulaJuridica.Trim(),
            ["direccion"] = direccion.Trim(),
            ["tipo_comida"] = tipo.ToString(),
        });
        return "Restaurante actualizado.";
    }

    public async Task<string> EliminarRestauranteAsync(int id)
    {
        var todos = await ObtenerRestaurantesAsync();
        if (todos.All(r => r.Id != id))
            return "Error: restaurante no encontrado.";
        var nPed = await ContarPedidosPorRestauranteAsync(id);
        if (nPed > 0)
            return "Error: hay pedidos asociados a este restaurante.";
        await DeleteExpectOk($"rest/v1/combos?id_restaurante=eq.{id}");
        await DeleteExpectOk($"rest/v1/restaurantes?id=eq.{id}");
        return "Restaurante eliminado.";
    }

    private async Task<int> ContarPedidosPorRestauranteAsync(int idRestaurante)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, $"rest/v1/pedidos?id_restaurante=eq.{idRestaurante}&select=id");
        var (ok, body, _) = await SendAsync(req);
        if (!ok || string.IsNullOrWhiteSpace(body)) return 0;
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetArrayLength();
    }

    public async Task<List<Combo>> ObtenerCombosAsync(int idRestaurante)
    {
        using var doc = await GetJsonDoc($"rest/v1/combos?id_restaurante=eq.{idRestaurante}&select=*&order=numero_combo.asc")
                     ?? JsonDocument.Parse("[]");
        var list = new List<Combo>();
        foreach (var row in doc.RootElement.EnumerateArray())
        {
            var cid = row.TryGetProperty("id", out var idc) ? idc.GetInt32() : list.Count + 1;
            list.Add(new Combo(
                cid,
                idRestaurante,
                row.GetProperty("numero_combo").GetInt32(),
                row.GetProperty("descripcion").GetString() ?? "",
                row.TryGetProperty("precio", out var pr) ? pr.GetDouble() : 0));
        }
        return list;
    }

    public async Task<string> AgregarComboAsync(int idRestaurante, string descripcion, double precio, int? numeroComboOpcional)
    {
        var rests = await ObtenerRestaurantesAsync();
        if (rests.All(r => r.Id != idRestaurante))
            return "Error: restaurante no encontrado.";
        if (string.IsNullOrWhiteSpace(descripcion))
            return "Error: descripcion obligatoria.";
        if (precio <= 0)
            return "Error: precio invalido.";
        var combos = await ObtenerCombosAsync(idRestaurante);
        var used = combos.Select(c => c.NumeroCombo).ToHashSet();
        int num;
        if (numeroComboOpcional.HasValue)
        {
            num = numeroComboOpcional.Value;
            if (num is < 1 or > 9)
                return "Error: numeroCombo debe estar entre 1 y 9.";
            if (used.Contains(num))
                return "Error: ya existe un combo con ese numero en este restaurante.";
        }
        else
        {
            num = Enumerable.Range(1, 9).FirstOrDefault(n => !used.Contains(n));
            if (num == 0)
                return "Error: el restaurante ya tiene el maximo de combos (9).";
        }
        await PostNoBodyExpectOk("rest/v1/combos", new[]
        {
            new Dictionary<string, object?>
            {
                ["id_restaurante"] = idRestaurante,
                ["numero_combo"] = num,
                ["descripcion"] = descripcion.Trim(),
                ["precio"] = precio,
            },
        });
        return $"Combo #{num} agregado.";
    }

    public async Task<string> EliminarComboAsync(int idRestaurante, int numeroCombo)
    {
        var rests = await ObtenerRestaurantesAsync();
        if (rests.All(r => r.Id != idRestaurante))
            return "Error: restaurante no encontrado.";
        if (numeroCombo is < 1 or > 9)
            return "Error: numeroCombo invalido.";
        var combos = await ObtenerCombosAsync(idRestaurante);
        if (combos.All(c => c.NumeroCombo != numeroCombo))
            return "Error: combo no encontrado.";
        await DeleteExpectOk($"rest/v1/combos?id_restaurante=eq.{idRestaurante}&numero_combo=eq.{numeroCombo}");
        return "Combo eliminado.";
    }

    public async Task<string> ActualizarComboAsync(int idRestaurante, int numeroCombo, string descripcion, double precio)
    {
        var rests = await ObtenerRestaurantesAsync();
        if (rests.All(r => r.Id != idRestaurante))
            return "Error: restaurante no encontrado.";
        if (numeroCombo is < 1 or > 9)
            return "Error: numeroCombo invalido.";
        var combos = await ObtenerCombosAsync(idRestaurante);
        if (combos.All(c => c.NumeroCombo != numeroCombo))
            return "Error: combo no encontrado.";
        if (string.IsNullOrWhiteSpace(descripcion))
            return "Error: descripcion obligatoria.";
        if (precio <= 0)
            return "Error: precio invalido.";
        await PatchExpectOk($"rest/v1/combos?id_restaurante=eq.{idRestaurante}&numero_combo=eq.{numeroCombo}", new Dictionary<string, object?>
        {
            ["descripcion"] = descripcion.Trim(),
            ["precio"] = precio,
        });
        return "Combo actualizado.";
    }

    private static Cliente MapCliente(JsonElement row)
    {
        var suspendido = row.TryGetProperty("suspendido", out var sus) && sus.GetBoolean();
        return new Cliente(
            row.TryGetProperty("id", out var idEl) ? idEl.GetInt32() : 0,
            row.GetProperty("cedula").GetString() ?? "",
            row.GetProperty("nombre").GetString() ?? "",
            row.GetProperty("direccion").GetString() ?? "",
            row.TryGetProperty("tarjeta", out var tj) ? tj.GetString() ?? "" : "",
            row.TryGetProperty("celular", out var cel) ? cel.GetString() ?? "" : "",
            row.TryGetProperty("correo", out var co) ? co.GetString() ?? "" : "",
            suspendido ? EstadoCliente.SUSPENDIDO : EstadoCliente.ACTIVO);
    }

    public async Task<List<Cliente>> ObtenerClientesActivosAsync()
    {
        using var doc = await GetJsonDoc("rest/v1/clientes?select=*&suspendido=eq.false&order=nombre.asc")
                     ?? JsonDocument.Parse("[]");
        return doc.RootElement.EnumerateArray().Select(MapCliente).ToList();
    }

    public async Task<List<Cliente>> ObtenerClientesSuspendidosAsync()
    {
        using var doc = await GetJsonDoc("rest/v1/clientes?select=*&suspendido=eq.true&order=nombre.asc")
                     ?? JsonDocument.Parse("[]");
        return doc.RootElement.EnumerateArray().Select(MapCliente).ToList();
    }

    public async Task<string> VerificarClienteAccesoAsync(string cedula)
    {
        using var doc = await GetJsonDoc("rest/v1/clientes?cedula=eq." + Uri.EscapeDataString(cedula) + "&select=suspendido");
        if (doc == null || doc.RootElement.GetArrayLength() == 0)
            return "NO_REGISTRADO";
        var susp = doc.RootElement[0].GetProperty("suspendido").GetBoolean();
        return susp ? "SUSPENDIDO" : "ACTIVO";
    }

    public async Task<string> RegistrarClienteAsync(string cedula, string nombre, string direccion, string tarjeta, string celular, string correo)
    {
        using var doc = await GetJsonDoc("rest/v1/clientes?cedula=eq." + Uri.EscapeDataString(cedula) + "&select=cedula");
        if (doc != null && doc.RootElement.GetArrayLength() > 0)
            return $"Error: cedula {cedula} ya registrada.";
        await PostNoBodyExpectOk("rest/v1/clientes", new[]
        {
            new Dictionary<string, object?>
            {
                ["cedula"] = cedula,
                ["nombre"] = nombre,
                ["direccion"] = direccion,
                ["tarjeta"] = tarjeta,
                ["celular"] = celular,
                ["correo"] = correo,
                ["suspendido"] = false,
            },
        });
        return $"Cliente '{nombre}' registrado.";
    }

    public async Task<string> ActualizarClienteAsync(string cedula, string nombre, string direccion, string tarjeta, string celular, string correo, bool suspendido)
    {
        using var exist = await GetJsonDoc("rest/v1/clientes?cedula=eq." + Uri.EscapeDataString(cedula) + "&select=cedula");
        if (exist == null || exist.RootElement.GetArrayLength() == 0)
            return "Error: cliente no encontrado.";
        await PatchExpectOk("rest/v1/clientes?cedula=eq." + Uri.EscapeDataString(cedula), new Dictionary<string, object?>
        {
            ["nombre"] = nombre,
            ["direccion"] = direccion,
            ["tarjeta"] = tarjeta,
            ["celular"] = celular,
            ["correo"] = correo,
            ["suspendido"] = suspendido,
        });
        return "Cliente actualizado.";
    }

    public async Task<string> EliminarClienteAsync(string cedula)
    {
        using var exist = await GetJsonDoc("rest/v1/clientes?cedula=eq." + Uri.EscapeDataString(cedula) + "&select=cedula");
        if (exist == null || exist.RootElement.GetArrayLength() == 0)
            return "Error: cliente no encontrado.";
        var n = await ContarPedidosPorClienteCedulaAsync(cedula);
        if (n > 0)
            return "Error: el cliente tiene pedidos registrados.";
        await DeleteExpectOk("rest/v1/clientes?cedula=eq." + Uri.EscapeDataString(cedula));
        return "Cliente eliminado.";
    }

    private async Task<int> ContarPedidosPorClienteCedulaAsync(string cedula)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get,
            "rest/v1/pedidos?cedula_cliente=eq." + Uri.EscapeDataString(cedula) + "&select=id");
        var (ok, body, _) = await SendAsync(req);
        if (!ok || string.IsNullOrWhiteSpace(body)) return 0;
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetArrayLength();
    }

    private static Repartidor MapRepartidor(JsonElement row, int fallbackId)
    {
        var id = row.TryGetProperty("id", out var idEl) ? idEl.GetInt32() : fallbackId;
        var nombre = row.TryGetProperty("nombre", out var nom) ? nom.GetString() ?? "" : "";
        var cedula = row.TryGetProperty("cedula", out var ce) ? ce.GetString() ?? "" : "";
        var correo = row.TryGetProperty("correo", out var co) ? co.GetString() ?? "" : "";
        var direccion = row.TryGetProperty("direccion", out var di) ? di.GetString() ?? "" : "";
        var celular = row.TryGetProperty("celular", out var cel) ? cel.GetString() ?? "" : "";
        var tarjeta = row.TryGetProperty("tarjeta", out var ta) ? ta.GetString() ?? "" : "";
        var am = row.TryGetProperty("amonestaciones", out var amon) ? amon.GetInt32() : 0;
        return new Repartidor(id, nombre, cedula, correo, direccion, celular, tarjeta,
            EstadoRepartidor.DISPONIBLE, 0, 0, am);
    }

    public async Task<List<Repartidor>> ObtenerRepartidoresAsync()
    {
        using var doc = await GetJsonDoc("rest/v1/repartidores?select=*&order=nombre.asc") ?? JsonDocument.Parse("[]");
        var list = new List<Repartidor>();
        foreach (var row in doc.RootElement.EnumerateArray())
            list.Add(MapRepartidor(row, list.Count + 1));
        return list;
    }

    public async Task<List<Repartidor>> ObtenerRepartidoresCeroAmonestacionesAsync()
    {
        var all = await ObtenerRepartidoresAsync();
        return all.Where(r => r.NumeroAmonestaciones == 0).ToList();
    }

    public async Task<string> RegistrarRepartidorAsync(string cedula, string nombre, string correo, string direccion, string celular, string tarjeta)
    {
        var rests = await ObtenerRestaurantesAsync();
        if (rests.Count == 0)
            return "Error: debe existir al menos un restaurante para registrar repartidores.";
        var idRestFallback = rests[0].Id;
        var reps = await ObtenerRepartidoresAsync();
        if (reps.Any(r => string.Equals(r.Cedula, cedula, StringComparison.OrdinalIgnoreCase)))
            return $"Error: cedula {cedula} ya registrada.";
        await PostNoBodyExpectOk("rest/v1/repartidores", new[]
        {
            new Dictionary<string, object?>
            {
                ["id_restaurante"] = idRestFallback,
                ["cedula"] = cedula,
                ["nombre"] = nombre,
                ["correo"] = correo,
                ["direccion"] = direccion,
                ["celular"] = celular,
                ["tarjeta"] = tarjeta,
                ["amonestaciones"] = 0,
            },
        });
        return $"Repartidor '{nombre}' registrado.";
    }

    public async Task<string> ActualizarRepartidorAsync(int id, string cedula, string nombre, string correo, string direccion, string celular, string tarjeta, int amonestaciones)
    {
        var reps = await ObtenerRepartidoresAsync();
        var cur = reps.FirstOrDefault(r => r.Id == id);
        if (cur == null)
            return "Error: repartidor no encontrado.";
        await PatchExpectOk($"rest/v1/repartidores?id=eq.{id}", new Dictionary<string, object?>
        {
            ["cedula"] = cedula,
            ["nombre"] = nombre,
            ["correo"] = correo,
            ["direccion"] = direccion,
            ["celular"] = celular,
            ["tarjeta"] = tarjeta,
            ["amonestaciones"] = amonestaciones,
        });
        return "Repartidor actualizado.";
    }

    public async Task<string> EliminarRepartidorAsync(int id)
    {
        var reps = await ObtenerRepartidoresAsync();
        if (reps.All(r => r.Id != id))
            return "Error: repartidor no encontrado.";
        await DeleteExpectOk($"rest/v1/repartidores?id=eq.{id}");
        return "Repartidor eliminado.";
    }

    public async Task<List<object>> ListarPedidosParaApiAsync()
    {
        using var doc = await GetJsonDoc("rest/v1/pedidos?select=*,items_pedido(*)&order=id.asc") ?? JsonDocument.Parse("[]");
        var list = new List<object>();
        foreach (var row in doc.RootElement.EnumerateArray())
        {
            var items = new List<object>();
            if (row.TryGetProperty("items_pedido", out var itArr) && itArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var it in itArr.EnumerateArray())
                {
                    items.Add(new
                    {
                        numeroCombo = it.TryGetProperty("numero_combo", out var nc) ? nc.GetInt32() : 0,
                        descripcion = it.TryGetProperty("descripcion", out var de) ? de.GetString() ?? "" : "",
                        precioUnitario = it.TryGetProperty("precio_unitario", out var pu) ? pu.GetDouble() : 0,
                        cantidad = it.TryGetProperty("cantidad", out var ca) ? ca.GetInt32() : 0,
                    });
                }
            }

            var estadoStr = row.TryGetProperty("estado", out var est) ? est.GetString() ?? "" : "";
            var idRep = row.TryGetProperty("id_repartidor", out var ir)
                ? (ir.ValueKind == JsonValueKind.Null ? 0 : ir.GetInt32())
                : 0;
            list.Add(new
            {
                id = row.GetProperty("id").GetInt32(),
                cedulaCliente = row.TryGetProperty("cedula_cliente", out var cc) ? cc.GetString() ?? "" : "",
                idRestaurante = row.TryGetProperty("id_restaurante", out var idr) ? idr.GetInt32() : 0,
                nombreRestaurante = row.TryGetProperty("nombre_restaurante", out var nr) ? nr.GetString() ?? "" : "",
                idRepartidor = idRep,
                distanciaKm = row.TryGetProperty("distancia_km", out var dk) ? dk.GetDouble() : 0,
                esFeriado = row.TryGetProperty("es_feriado", out var ef) && ef.GetBoolean(),
                mensaje = row.TryGetProperty("mensaje", out var ms)
                    ? ms.GetString() ?? ""
                    : $"Estado: {estadoStr} · Repartidor #{idRep}",
                observacion = row.TryGetProperty("observacion", out var ob) ? ob.GetString() ?? "" : "",
                items,
            });
        }
        return list;
    }

    public async Task<(bool Ok, string Mensaje, int? IdPedido)> RealizarPedidoAsync(string cedulaCliente, int idRestaurante,
        List<(int NumeroCombo, string Descripcion, double PrecioUnitario, int Cantidad)> items, double distanciaKm, bool esFeriado,
        string? nombreRestauranteOpcional)
    {
        var rests = await ObtenerRestaurantesAsync();
        var r = rests.FirstOrDefault(x => x.Id == idRestaurante);
        var nombreRest = string.IsNullOrWhiteSpace(nombreRestauranteOpcional) ? r?.Nombre ?? "" : nombreRestauranteOpcional.Trim();
        var row = await PostReturningRow("rest/v1/pedidos", new[]
        {
            new Dictionary<string, object?>
            {
                ["cedula_cliente"] = cedulaCliente,
                ["id_restaurante"] = idRestaurante,
                ["nombre_restaurante"] = nombreRest,
                ["distancia_km"] = distanciaKm,
                ["es_feriado"] = esFeriado,
                ["mensaje"] = "Pedido registrado.",
                ["observacion"] = "",
            },
        });
        if (row == null || !row.Value.TryGetProperty("id", out var idEl))
            return (false, "Error al crear pedido.", null);
        var pid = idEl.GetInt32();
        foreach (var it in items)
        {
            await PostNoBodyExpectOk("rest/v1/items_pedido", new[]
            {
                new Dictionary<string, object?>
                {
                    ["pedido_id"] = pid,
                    ["numero_combo"] = it.NumeroCombo,
                    ["descripcion"] = string.IsNullOrWhiteSpace(it.Descripcion) ? $"Combo {it.NumeroCombo}" : it.Descripcion,
                    ["precio_unitario"] = it.PrecioUnitario,
                    ["cantidad"] = it.Cantidad,
                },
            });
        }
        return (true, $"Pedido #{pid} creado.", pid);
    }

    public async Task<string> MarcarEntregadoAsync(int idPedido, int idRepartidor)
    {
        var patch = new Dictionary<string, object?>
        {
            ["estado"] = "ENTREGADO",
            ["mensaje"] = $"Pedido #{idPedido} · ENTREGADO",
        };
        if (idRepartidor > 0)
            patch["id_repartidor"] = idRepartidor;
        try
        {
            await PatchExpectOk($"rest/v1/pedidos?id=eq.{idPedido}", patch);
        }
        catch
        {
            await PatchExpectOk($"rest/v1/pedidos?id=eq.{idPedido}", new Dictionary<string, object?>
            {
                ["mensaje"] = $"Estado: ENTREGADO · Repartidor #{idRepartidor} · Pedido #{idPedido}",
            });
        }
        return $"Pedido #{idPedido} marcado entregado.";
    }

    public async Task<string> ActualizarObservacionPedidoAsync(int idPedido, string observacion)
    {
        await PatchExpectOk($"rest/v1/pedidos?id=eq.{idPedido}", new Dictionary<string, object?> { ["observacion"] = observacion ?? "" });
        return "Observacion actualizada.";
    }

    public async Task<string> EliminarPedidoAsync(int idPedido)
    {
        await DeleteExpectOk($"rest/v1/pedidos?id=eq.{idPedido}");
        return "Pedido eliminado.";
    }

    /// <summary>Reportes compatibles con ReportesApiController (datos desde pedidos Supabase).</summary>
    public async Task<string> ReporteRestauranteMasPedidosAsync()
    {
        var ((maxId, maxN), rests) = await RestPedidosCountsAsync();
        if (maxN < 0 || rests.Count == 0) return "Sin datos.";
        var nombre = rests.FirstOrDefault(r => r.Id == maxId)?.Nombre ?? $"#{maxId}";
        return $"Restaurante con mas pedidos: {nombre} ({maxN} pedidos)";
    }

    public async Task<string> ReporteRestauranteMenosPedidosAsync()
    {
        var ((minId, minN), rests) = await RestPedidosCountsAsync(min: true);
        if (minN == int.MaxValue || rests.Count == 0) return "Sin datos.";
        var nombre = rests.FirstOrDefault(r => r.Id == minId)?.Nombre ?? $"#{minId}";
        return $"Restaurante con menos pedidos: {nombre} ({minN} pedidos)";
    }

    public async Task<List<(string nombre, double monto)>> ReporteMontoPorRestauranteAsync()
    {
        using var doc = await GetJsonDoc("rest/v1/pedidos?select=*,items_pedido(*)") ?? JsonDocument.Parse("[]");
        var montos = new Dictionary<int, double>();
        foreach (var row in doc.RootElement.EnumerateArray())
        {
            var idr = row.TryGetProperty("id_restaurante", out var ir) ? ir.GetInt32() : 0;
            if (!montos.ContainsKey(idr)) montos[idr] = 0;
            if (row.TryGetProperty("items_pedido", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var it in items.EnumerateArray())
                {
                    var pu = it.TryGetProperty("precio_unitario", out var p) ? p.GetDouble() : 0;
                    var ca = it.TryGetProperty("cantidad", out var c) ? c.GetInt32() : 0;
                    montos[idr] += pu * ca;
                }
            }
        }
        var rests = await ObtenerRestaurantesAsync();
        return montos.Select(kv =>
        {
            var nombre = rests.FirstOrDefault(r => r.Id == kv.Key)?.Nombre ?? $"Restaurante #{kv.Key}";
            return (nombre, kv.Value);
        }).ToList();
    }

    public async Task<double> ReporteMontoTotalAsync()
    {
        var rows = await ReporteMontoPorRestauranteAsync();
        return rows.Sum(x => x.monto);
    }

    public Task<List<string>> ReporteQuejasPorRepartidorAsync() =>
        Task.FromResult(new List<string> { "Sin tabla de quejas en Supabase (no aplicable)." });

    public async Task<List<string>> ReportePedidosPorClienteAsync()
    {
        using var doc = await GetJsonDoc("rest/v1/pedidos?select=id,cedula_cliente,mensaje,estado&order=id.asc") ?? JsonDocument.Parse("[]");
        var porCedula = new Dictionary<string, List<(int id, string msg, string est)>>();
        foreach (var row in doc.RootElement.EnumerateArray())
        {
            var ced = row.TryGetProperty("cedula_cliente", out var c) ? c.GetString() ?? "" : "";
            if (!porCedula.ContainsKey(ced)) porCedula[ced] = new List<(int, string, string)>();
            var pid = row.GetProperty("id").GetInt32();
            var msg = row.TryGetProperty("mensaje", out var m) ? m.GetString() ?? "" : "";
            var est = row.TryGetProperty("estado", out var e) ? e.GetString() ?? "" : "";
            porCedula[ced].Add((pid, msg, est));
        }
        using var cliDoc = await GetJsonDoc("rest/v1/clientes?select=nombre,cedula") ?? JsonDocument.Parse("[]");
        var nombrePorCedula = new Dictionary<string, string>();
        foreach (var c in cliDoc.RootElement.EnumerateArray())
            nombrePorCedula[c.GetProperty("cedula").GetString() ?? ""] = c.GetProperty("nombre").GetString() ?? "";

        var lineas = new List<string>();
        foreach (var kv in porCedula.OrderBy(k => k.Key))
        {
            var nombre = nombrePorCedula.TryGetValue(kv.Key, out var n) ? n : kv.Key;
            lineas.Add($"\nCliente: {nombre} (Cedula: {kv.Key})");
            foreach (var p in kv.Value)
                lineas.Add($"  Pedido #{p.id} | {p.msg} | Estado: {p.est}");
        }
        return lineas;
    }

    public async Task<string> ReporteClienteMasPedidosAsync()
    {
        using var doc = await GetJsonDoc("rest/v1/pedidos?select=cedula_cliente") ?? JsonDocument.Parse("[]");
        var counts = new Dictionary<string, int>();
        foreach (var row in doc.RootElement.EnumerateArray())
        {
            var ced = row.TryGetProperty("cedula_cliente", out var c) ? c.GetString() ?? "" : "";
            counts[ced] = counts.TryGetValue(ced, out var n) ? n + 1 : 1;
        }
        if (counts.Count == 0) return "Sin datos.";
        var top = counts.OrderByDescending(kv => kv.Value).First();
        using var cliDoc = await GetJsonDoc("rest/v1/clientes?cedula=eq." + Uri.EscapeDataString(top.Key) + "&select=nombre,cedula");
        var nombre = "";
        if (cliDoc != null && cliDoc.RootElement.GetArrayLength() > 0)
            nombre = cliDoc.RootElement[0].GetProperty("nombre").GetString() ?? "";
        return $"Cliente con mas pedidos: {nombre} (Cedula: {top.Key}) con {top.Value} pedidos.";
    }

    public async Task<string> ReporteHoraPicoAsync()
    {
        using var doc = await GetJsonDoc("rest/v1/pedidos?select=created_at") ?? JsonDocument.Parse("[]");
        var hourCount = new Dictionary<int, int>();
        foreach (var row in doc.RootElement.EnumerateArray())
        {
            if (!row.TryGetProperty("created_at", out var ca)) continue;
            var s = ca.GetString();
            if (string.IsNullOrEmpty(s)) continue;
            if (!DateTime.TryParse(s, out var dt)) continue;
            var h = dt.Hour;
            hourCount[h] = hourCount.TryGetValue(h, out var n) ? n + 1 : 1;
        }
        if (hourCount.Count == 0) return "Sin datos.";
        var top = hourCount.OrderByDescending(kv => kv.Value).First();
        return $"Hora pico: {top.Key}:00 hrs ({top.Value} pedidos)";
    }

    private async Task<((int id, int n) best, List<Restaurante> rests)> RestPedidosCountsAsync(bool min = false)
    {
        using var doc = await GetJsonDoc("rest/v1/pedidos?select=id_restaurante") ?? JsonDocument.Parse("[]");
        var counts = new Dictionary<int, int>();
        foreach (var row in doc.RootElement.EnumerateArray())
        {
            var idr = row.TryGetProperty("id_restaurante", out var ir) ? ir.GetInt32() : 0;
            counts[idr] = counts.TryGetValue(idr, out var n) ? n + 1 : 1;
        }
        var rests = await ObtenerRestaurantesAsync();
        foreach (var r in rests)
            counts.TryAdd(r.Id, 0);
        if (counts.Count == 0)
            return ((-1, min ? int.MaxValue : -1), rests);
        if (min)
        {
            var kv = counts.OrderBy(k => k.Value).ThenBy(k => k.Key).First();
            return ((kv.Key, kv.Value), rests);
        }
        else
        {
            var kv = counts.OrderByDescending(k => k.Value).ThenBy(k => k.Key).First();
            return ((kv.Key, kv.Value), rests);
        }
    }
}
