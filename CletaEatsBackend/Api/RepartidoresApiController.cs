using Microsoft.AspNetCore.Mvc;
using CletaEatsBackend.Infra;

namespace CletaEatsBackend.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepartidoresApiController : ControllerBase
    {
        private readonly SupabaseRestService _sb;

        public RepartidoresApiController(SupabaseRestService sb) => _sb = sb;

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarRepartidorRequest req)
        {
            if (req == null) return BadRequest("Datos requeridos.");
            try
            {
                var msg = await _sb.RegistrarRepartidorAsync(
                    req.Cedula ?? "", req.Nombre ?? "", req.Correo ?? "",
                    req.Direccion ?? "", req.Celular ?? "", req.Tarjeta ?? "");
                if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
                return Ok(new { mensaje = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Todos()
        {
            try
            {
                return Ok(await _sb.ObtenerRepartidoresAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("cero-amonestaciones")]
        public async Task<IActionResult> CeroAmonestaciones()
        {
            try
            {
                return Ok(await _sb.ObtenerRepartidoresCeroAmonestacionesAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarRepartidorRequest? req)
        {
            if (req == null) return BadRequest(new { mensaje = "Datos requeridos." });
            try
            {
                var msg = await _sb.ActualizarRepartidorAsync(
                    id,
                    req.Cedula ?? "",
                    req.Nombre ?? "",
                    req.Correo ?? "",
                    req.Direccion ?? "",
                    req.Celular ?? "",
                    req.Tarjeta ?? "",
                    req.Amonestaciones);
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
                var msg = await _sb.EliminarRepartidorAsync(id);
                if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
                return Ok(new { mensaje = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }

    public class ActualizarRepartidorRequest
    {
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public string? Celular { get; set; }
        public string? Tarjeta { get; set; }
        public int Amonestaciones { get; set; }
    }

    public class RegistrarRepartidorRequest
    {
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public string? Celular { get; set; }
        public string? Tarjeta { get; set; }
    }
}
