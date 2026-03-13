using CletaEatsBackend.LogicaNegocio;

namespace CletaEatsBackend.Control
{
    public class RepartidorController
    {
        private readonly RepartidorService _service = new();

        public string Registrar(string cedula, string nombre, string correo, string direccion, string celular, string tarjeta)
        {
            return _service.Registrar(cedula, nombre, correo, direccion, celular, tarjeta);
        }

        public void MostrarRepartidoresConCeroAmonestaciones()
        {
            Console.WriteLine("\n=== Repartidores con 0 amonestaciones ===");
            foreach (var r in _service.ObtenerConCeroAmonestaciones())
                Console.WriteLine(r);
        }

        public void MostrarTodos()
        {
            Console.WriteLine("\n=== Repartidores ===");
            foreach (var r in _service.ObtenerTodos())
                Console.WriteLine(r);
        }
    }
}
