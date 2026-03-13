using Microsoft.AspNetCore.Mvc;
using CletaEatsBackend.Control;

namespace CletaEatsBackend.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesApiController : ControllerBase
    {
        private readonly ReporteController _ctrl = new();

        [HttpGet("restaurante-mas-pedidos")]
        public IActionResult RestauranteConMasPedidos() =>
            Ok(new { texto = _ctrl.GetRestauranteConMasPedidos() });

        [HttpGet("restaurante-menos-pedidos")]
        public IActionResult RestauranteConMenosPedidos() =>
            Ok(new { texto = _ctrl.GetRestauranteConMenosPedidos() });

        [HttpGet("monto-por-restaurante")]
        public IActionResult MontoPorRestaurante() =>
            Ok(_ctrl.GetMontoPorRestaurante().Select(x => new { nombre = x.nombre, monto = x.monto }));

        [HttpGet("monto-total")]
        public IActionResult MontoTotal() =>
            Ok(new { total = _ctrl.GetMontoTotalGeneral() });

        [HttpGet("quejas-por-repartidor")]
        public IActionResult QuejasPorRepartidor() =>
            Ok(_ctrl.GetQuejasPorRepartidor());

        [HttpGet("pedidos-por-cliente")]
        public IActionResult PedidosPorCliente() =>
            Ok(_ctrl.GetPedidosPorCliente());

        [HttpGet("cliente-mas-pedidos")]
        public IActionResult ClienteConMasPedidos() =>
            Ok(new { texto = _ctrl.GetClienteConMasPedidos() });

        [HttpGet("hora-pico")]
        public IActionResult HoraPico() =>
            Ok(new { texto = _ctrl.GetHoraPico() });
    }
}
