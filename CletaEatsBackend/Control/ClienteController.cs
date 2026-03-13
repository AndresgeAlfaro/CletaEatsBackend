using CletaEatsBackend.LogicaNegocio;

namespace CletaEatsBackend.Control
{
    public class ClienteController
    {
        private readonly ClienteService _service = new();

        public string Registrar(string cedula, string nombre, string dir, string tarjeta, string cel, string correo)
        {
            return _service.Registrar(cedula, nombre, dir, tarjeta, cel, correo);
        }

        public string VerificarAcceso(string cedula)
        {
            return _service.VerificarAcceso(cedula);
        }

        public void MostrarActivos()
        {
            Console.WriteLine("\n=== Clientes ACTIVOS ===");
            foreach (var c in _service.ObtenerActivos())
                Console.WriteLine(c);
        }

        public void MostrarSuspendidos()
        {
            Console.WriteLine("\n=== Clientes SUSPENDIDOS ===");
            foreach (var c in _service.ObtenerSuspendidos())
                Console.WriteLine(c);
        }
    }
}
