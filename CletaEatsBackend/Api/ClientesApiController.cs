using Microsoft.AspNetCore.Mvc;
using CletaEatsBackend.Control;
using CletaEatsBackend.Modelo;

namespace CletaEatsBackend.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesApiController : ControllerBase
    {
        private readonly ClienteController _ctrl = new();

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] RegistrarClienteRequest req)
        {
            if (req == null) return BadRequest("Datos requeridos.");
            var msg = _ctrl.Registrar(req.Cedula ?? "", req.Nombre ?? "", req.Direccion ?? "", req.Tarjeta ?? "", req.Celular ?? "", req.Correo ?? "");
            if (msg.StartsWith("Error")) return BadRequest(new { mensaje = msg });
            return Ok(new { mensaje = msg });
        }

        [HttpGet("verificar/{cedula}")]
        public IActionResult Verificar(string cedula)
        {
            var estado = _ctrl.VerificarAcceso(cedula);
            return Ok(new { cedula, estado });
        }

        [HttpGet("activos")]
        public IActionResult Activos()
        {
            var lista = _ctrl.GetActivos();
            return Ok(lista);
        }

        [HttpGet("suspendidos")]
        public IActionResult Suspendidos()
        {
            var lista = _ctrl.GetSuspendidos();
            return Ok(lista);
        }
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
