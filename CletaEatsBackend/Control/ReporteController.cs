using CletaEatsBackend.LogicaNegocio;

namespace CletaEatsBackend.Control
{
    public class ReporteController
    {
        private readonly ReporteService _service = new();

        public string GetRestauranteConMasPedidos() => _service.RestauranteConMasPedidos();
        public List<(string nombre, double monto)> GetMontoPorRestaurante() => _service.MontoPorRestaurante();
        public double GetMontoTotalGeneral() => _service.MontoTotalGeneral();
        public string GetRestauranteConMenosPedidos() => _service.RestauranteConMenosPedidos();
        public List<string> GetQuejasPorRepartidor() => _service.QuejasPorRepartidor();
        public List<string> GetPedidosPorCliente() => _service.PedidosPorCliente();
        public string GetClienteConMasPedidos() => _service.ClienteConMasPedidos();
        public string GetHoraPico() => _service.HoraPico();

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
