using Microsoft.AspNetCore.Mvc;
using CletaEatsBackend.Infra;

namespace CletaEatsBackend.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosApiController : ControllerBase
    {
        private readonly SupabaseRestService _sb;

        public PedidosApiController(SupabaseRestService sb) => _sb = sb;

        /// <summary>Listado de pedidos (Supabase).</summary>
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            try
            {
                return Ok(await _sb.ListarPedidosParaApiAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("realizar")]
        public async Task<IActionResult> Realizar([FromBody] RealizarPedidoRequest req)
        {
            if (req == null || req.Items == null || req.Items.Count == 0)
                return BadRequest(new { mensaje = "Se requieren items del pedido." });
            var items = req.Items.Select(i => (i.NumeroCombo, i.Descripcion ?? $"Combo {i.NumeroCombo}", i.PrecioUnitario, i.Cantidad)).ToList();
            try
            {
                var (ok, msg, id) = await _sb.RealizarPedidoAsync(
                    req.CedulaCliente ?? "",
                    req.IdRestaurante,
                    items,
                    req.DistanciaKm,
                    req.EsFeriado,
                    req.NombreRestaurante);
                if (!ok) return BadRequest(new { mensaje = msg });
                return Ok(new { mensaje = msg, id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("marcar-entregado")]
        public async Task<IActionResult> MarcarEntregado([FromBody] MarcarEntregadoRequest req)
        {
            if (req == null) return BadRequest("idPedido e idRepartidor requeridos.");
            try
            {
                var msg = await _sb.MarcarEntregadoAsync(req.IdPedido, req.IdRepartidor);
                return Ok(new { mensaje = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPatch("{id:int}/observacion")]
        public async Task<IActionResult> ActualizarObservacion(int id, [FromBody] ObservacionPedidoRequest req)
        {
            if (req == null) return BadRequest(new { mensaje = "Body requerido." });
            try
            {
                var msg = await _sb.ActualizarObservacionPedidoAsync(id, req.Observacion ?? "");
                if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
                return Ok(new { mensaje = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var msg = await _sb.EliminarPedidoAsync(id);
                if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
                return Ok(new { mensaje = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }

    public class ObservacionPedidoRequest
    {
        public string? Observacion { get; set; }
    }

    public class RealizarPedidoRequest
    {
        public string? CedulaCliente { get; set; }
        public int IdRestaurante { get; set; }
        /// <summary>Opcional; si falta se toma del restaurante en Supabase.</summary>
        public string? NombreRestaurante { get; set; }
        public double DistanciaKm { get; set; } = 1;
        public bool EsFeriado { get; set; }
        public List<ItemPedidoRequest>? Items { get; set; }
    }

    public class ItemPedidoRequest
    {
        public int NumeroCombo { get; set; }
        public string? Descripcion { get; set; }
        public double PrecioUnitario { get; set; }
        public int Cantidad { get; set; } = 1;
    }

    public class MarcarEntregadoRequest
    {
        public int IdPedido { get; set; }
        public int IdRepartidor { get; set; }
    }
}
