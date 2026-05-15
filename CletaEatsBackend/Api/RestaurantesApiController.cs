using Microsoft.AspNetCore.Mvc;
using CletaEatsBackend.Infra;

namespace CletaEatsBackend.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantesApiController : ControllerBase
    {
        private readonly SupabaseRestService _sb;

        public RestaurantesApiController(SupabaseRestService sb) => _sb = sb;

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarRestauranteRequest req)
        {
            if (req == null) return BadRequest("Datos requeridos.");
            try
            {
                var msg = await _sb.RegistrarRestauranteAsync(
                    req.Nombre ?? "", req.CedulaJuridica ?? "", req.Direccion ?? "", req.TipoComida ?? "");
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
                return Ok(await _sb.ObtenerRestaurantesAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] RegistrarRestauranteRequest? req)
        {
            if (req == null) return BadRequest(new { mensaje = "Datos requeridos." });
            try
            {
                var msg = await _sb.ActualizarRestauranteAsync(id, req.Nombre ?? "", req.CedulaJuridica ?? "", req.Direccion ?? "", req.TipoComida ?? "");
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
                var msg = await _sb.EliminarRestauranteAsync(id);
                if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
                return Ok(new { mensaje = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("{idRestaurante:int}/combos")]
        public async Task<IActionResult> Combos(int idRestaurante)
        {
            try
            {
                return Ok(await _sb.ObtenerCombosAsync(idRestaurante));
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPost("{idRestaurante:int}/combos")]
        public async Task<IActionResult> AgregarCombo(int idRestaurante, [FromBody] AgregarComboRequest? req)
        {
            if (req == null) return BadRequest(new { mensaje = "Datos requeridos." });
            try
            {
                var msg = await _sb.AgregarComboAsync(idRestaurante, req.Descripcion ?? "", req.Precio, req.NumeroCombo);
                if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
                return Ok(new { mensaje = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{idRestaurante:int}/combos/{numeroCombo:int}")]
        public async Task<IActionResult> EliminarCombo(int idRestaurante, int numeroCombo)
        {
            try
            {
                var msg = await _sb.EliminarComboAsync(idRestaurante, numeroCombo);
                if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
                return Ok(new { mensaje = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{idRestaurante:int}/combos/{numeroCombo:int}")]
        public async Task<IActionResult> ActualizarCombo(int idRestaurante, int numeroCombo, [FromBody] ActualizarComboRequest? req)
        {
            if (req == null) return BadRequest(new { mensaje = "Datos requeridos." });
            try
            {
                var precio = req.Precio ?? 0;
                var msg = await _sb.ActualizarComboAsync(idRestaurante, numeroCombo, req.Descripcion ?? "", precio);
                if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
                return Ok(new { mensaje = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }

    public class AgregarComboRequest
    {
        public string? Descripcion { get; set; }
        public double Precio { get; set; }
        /// <summary>Opcional (1-9). Si no se envia, se asigna el primer slot libre.</summary>
        public int? NumeroCombo { get; set; }
    }

    public class ActualizarComboRequest
    {
        public string? Descripcion { get; set; }
        public double? Precio { get; set; }
    }

    public class RegistrarRestauranteRequest
    {
        public string? Nombre { get; set; }
        public string? CedulaJuridica { get; set; }
        public string? Direccion { get; set; }
        public string? TipoComida { get; set; }
    }
}
