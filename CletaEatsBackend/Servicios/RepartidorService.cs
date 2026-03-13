using CletaEatsBackend.AccesoDatos;
using CletaEatsBackend.Modelo;

namespace CletaEatsBackend.LogicaNegocio
{
    public class RepartidorService
    {
        private readonly RepartidorDAO _repartidorDAO = new();

        public string Registrar(string cedula, string nombre, string correo, string direccion, string celular, string tarjeta)
        {
            var todos = _repartidorDAO.ObtenerTodos();
            if (todos.Any(r => r.Cedula == cedula))
                return $"Error: cedula {cedula} ya registrada.";
            var rep = new Repartidor
            {
                Nombre = nombre,
                Cedula = cedula,
                CorreoElectronico = correo,
                DireccionExacta = direccion,
                NumeroCelular = celular,
                NumeroTarjeta = tarjeta,
                Estado = EstadoRepartidor.DISPONIBLE,
                DistanciaDelPedido = 0,
                KilometrosDiarios = 0,
                NumeroAmonestaciones = 0
            };
            return _repartidorDAO.Insertar(rep)
                ? $"Repartidor '{nombre}' registrado."
                : "Error al guardar.";
        }

        public List<Repartidor> ObtenerTodos() => _repartidorDAO.ObtenerTodos();
        public List<Repartidor> ObtenerConCeroAmonestaciones() =>
            _repartidorDAO.ObtenerTodos().Where(r => r.NumeroAmonestaciones == 0).ToList();
    }
}
