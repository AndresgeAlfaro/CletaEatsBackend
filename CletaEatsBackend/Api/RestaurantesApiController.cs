using Microsoft.AspNetCore.Mvc;
using CletaEatsBackend.Control;
using CletaEatsBackend.Modelo;

namespace CletaEatsBackend.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantesApiController : ControllerBase
    {
        private readonly RestauranteController _ctrl = new();

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] RegistrarRestauranteRequest req)
        {
            if (req == null) return BadRequest("Datos requeridos.");
            var msg = _ctrl.Registrar(req.Nombre ?? "", req.CedulaJuridica ?? "", req.Direccion ?? "", req.TipoComida ?? "");
            if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
            return Ok(new { mensaje = msg });
        }

        [HttpGet]
        public IActionResult Todos()
        {
            return Ok(_ctrl.ObtenerTodos());
        }

        [HttpGet("{idRestaurante}/combos")]
        public IActionResult Combos(int idRestaurante)
        {
            return Ok(_ctrl.ObtenerCombos(idRestaurante));
        }
    }

    public class RegistrarRestauranteRequest
    {
        public string? Nombre { get; set; }
        public string? CedulaJuridica { get; set; }
        public string? Direccion { get; set; }
        public string? TipoComida { get; set; }
    }
}
