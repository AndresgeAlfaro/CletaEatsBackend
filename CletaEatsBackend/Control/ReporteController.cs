using CletaEatsBackend.LogicaNegocio;

namespace CletaEatsBackend.Control
{
    public class ReporteController
    {
        private readonly ReporteService _service = new();

        public void RestauranteConMasPedidos()
        {
            Console.WriteLine(_service.RestauranteConMasPedidos());
        }

        public void MontoPorRestaurante()
        {
            Console.WriteLine("\n=== Monto por restaurante ===");
            foreach (var (nombre, monto) in _service.MontoPorRestaurante())
                Console.WriteLine($"{nombre}: {monto:C}");
        }

        public void MontoTotalGeneral()
        {
            Console.WriteLine($"Total general: {_service.MontoTotalGeneral():C}");
        }

        public void RestauranteConMenosPedidos()
        {
            Console.WriteLine(_service.RestauranteConMenosPedidos());
        }

        public void QuejasPorRepartidor()
        {
            Console.WriteLine("\n=== Quejas por repartidor ===");
            foreach (var linea in _service.QuejasPorRepartidor())
                Console.WriteLine(linea);
        }

        public void PedidosPorCliente()
        {
            Console.WriteLine("\n=== Pedidos por cliente ===");
            foreach (var linea in _service.PedidosPorCliente())
                Console.WriteLine(linea);
        }

        public void ClienteConMasPedidos()
        {
            Console.WriteLine(_service.ClienteConMasPedidos());
        }

        public void HoraPico()
        {
            Console.WriteLine(_service.HoraPico());
        }
    }
}
