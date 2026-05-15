using Microsoft.AspNetCore.Mvc;
using CletaEatsBackend.Infra;

namespace CletaEatsBackend.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesApiController : ControllerBase
    {
        private readonly SupabaseRestService _sb;

        public ReportesApiController(SupabaseRestService sb) => _sb = sb;

        [HttpGet("restaurante-mas-pedidos")]
        public async Task<IActionResult> RestauranteConMasPedidos()
        {
            try
            {
                return Ok(new { texto = await _sb.ReporteRestauranteMasPedidosAsync() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("restaurante-menos-pedidos")]
        public async Task<IActionResult> RestauranteConMenosPedidos()
        {
            try
            {
                return Ok(new { texto = await _sb.ReporteRestauranteMenosPedidosAsync() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("monto-por-restaurante")]
        public async Task<IActionResult> MontoPorRestaurante()
        {
            try
            {
                var rows = await _sb.ReporteMontoPorRestauranteAsync();
                return Ok(rows.Select(x => new { nombre = x.nombre, monto = x.monto }));
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("monto-total")]
        public async Task<IActionResult> MontoTotal()
        {
            try
            {
                return Ok(new { total = await _sb.ReporteMontoTotalAsync() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("quejas-por-repartidor")]
        public async Task<IActionResult> QuejasPorRepartidor()
        {
            try
            {
                return Ok(await _sb.ReporteQuejasPorRepartidorAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("pedidos-por-cliente")]
        public async Task<IActionResult> PedidosPorCliente()
        {
            try
            {
                return Ok(await _sb.ReportePedidosPorClienteAsync());
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("cliente-mas-pedidos")]
        public async Task<IActionResult> ClienteConMasPedidos()
        {
            try
            {
                return Ok(new { texto = await _sb.ReporteClienteMasPedidosAsync() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("hora-pico")]
        public async Task<IActionResult> HoraPico()
        {
            try
            {
                return Ok(new { texto = await _sb.ReporteHoraPicoAsync() });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
