using Microsoft.AspNetCore.Mvc;
using CletaEatsBackend.Infra;

namespace CletaEatsBackend.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesApiController : ControllerBase
    {
        private readonly SupabaseRestService _sb;

        public ClientesApiController(SupabaseRestService sb) => _sb = sb;

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarClienteRequest req)
        {
            if (req == null) return BadRequest("Datos requeridos.");
            try
            {
                var msg = await _sb.RegistrarClienteAsync(
                    req.Cedula ?? "", req.Nombre ?? "", req.Direccion ?? "",
                    req.Tarjeta ?? "", req.Celular ?? "", req.Correo ?? "");
                if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
                return Ok(new { mensaje = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("verificar/{cedula}")]
        public async Task<IActionResult> Verificar(string cedula)
        {
            try
            {
                var estado = await _sb.VerificarClienteAccesoAsync(cedula);
                return Ok(new { cedula, estado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("activos")]
        public async Task<IActionResult> Activos()
        {
            try
            {
                return Ok(await _sb.ObtenerClientesActivosAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("suspendidos")]
        public async Task<IActionResult> Suspendidos()
        {
            try
            {
                return Ok(await _sb.ObtenerClientesSuspendidosAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{cedula}")]
        public async Task<IActionResult> Actualizar(string cedula, [FromBody] ActualizarClienteRequest? req)
        {
            if (req == null) return BadRequest(new { mensaje = "Datos requeridos." });
            try
            {
                var msg = await _sb.ActualizarClienteAsync(
                    cedula,
                    req.Nombre ?? "",
                    req.Direccion ?? "",
                    req.Tarjeta ?? "",
                    req.Celular ?? "",
                    req.Correo ?? "",
                    req.Suspendido);
                if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
                return Ok(new { mensaje = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{cedula}")]
        public async Task<IActionResult> Eliminar(string cedula)
        {
            try
            {
                var msg = await _sb.EliminarClienteAsync(cedula);
                if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
                return Ok(new { mensaje = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }

    public class ActualizarClienteRequest
    {
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Tarjeta { get; set; }
        public string? Celular { get; set; }
        public string? Correo { get; set; }
        public bool Suspendido { get; set; }
    }

    public class RegistrarClienteRequest
    {
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Tarjeta { get; set; }
        public string? Celular { get; set; }
        public string? Correo { get; set; }
    }
}
