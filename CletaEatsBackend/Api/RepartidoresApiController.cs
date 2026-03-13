using Microsoft.AspNetCore.Mvc;
using CletaEatsBackend.Control;

namespace CletaEatsBackend.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepartidoresApiController : ControllerBase
    {
        private readonly RepartidorController _ctrl = new();

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] RegistrarRepartidorRequest req)
        {
            if (req == null) return BadRequest("Datos requeridos.");
            var msg = _ctrl.Registrar(req.Cedula ?? "", req.Nombre ?? "", req.Correo ?? "", req.Direccion ?? "", req.Celular ?? "", req.Tarjeta ?? "");
            if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
            return Ok(new { mensaje = msg });
        }

        [HttpGet]
        public IActionResult Todos()
        {
            return Ok(_ctrl.GetTodos());
        }

        [HttpGet("cero-amonestaciones")]
        public IActionResult CeroAmonestaciones()
        {
            return Ok(_ctrl.GetConCeroAmonestaciones());
        }
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
