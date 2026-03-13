using Microsoft.AspNetCore.Mvc;
using CletaEatsBackend.Control;
using CletaEatsBackend.Modelo;

namespace CletaEatsBackend.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosApiController : ControllerBase
    {
        private readonly PedidoController _ctrl = new();

        [HttpPost("realizar")]
        public IActionResult Realizar([FromBody] RealizarPedidoRequest req)
        {
            if (req == null || req.Items == null || req.Items.Count == 0)
                return BadRequest(new { mensaje = "Se requieren items del pedido." });
            var items = req.Items.Select(i => new ItemPedido
            {
                NumeroCombo = i.NumeroCombo,
                Descripcion = i.Descripcion ?? $"Combo {i.NumeroCombo}",
                PrecioUnitario = i.PrecioUnitario,
                Cantidad = i.Cantidad
            }).ToList();
            var (ok, msg) = _ctrl.RealizarPedido(req.CedulaCliente ?? "", req.IdRestaurante, items, req.DistanciaKm, req.EsFeriado);
            if (!ok) return BadRequest(new { mensaje = msg });
            return Ok(new { mensaje = msg });
        }

        [HttpPost("marcar-entregado")]
        public IActionResult MarcarEntregado([FromBody] MarcarEntregadoRequest req)
        {
            if (req == null) return BadRequest("idPedido e idRepartidor requeridos.");
            var msg = _ctrl.MarcarEntregado(req.IdPedido, req.IdRepartidor);
            return Ok(new { mensaje = msg });
        }
    }

    public class RealizarPedidoRequest
    {
        public string? CedulaCliente { get; set; }
        public int IdRestaurante { get; set; }
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
