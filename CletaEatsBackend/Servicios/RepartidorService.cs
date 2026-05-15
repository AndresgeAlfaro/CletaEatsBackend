using CletaEatsBackend.AccesoDatos;
using CletaEatsBackend.Modelo;

namespace CletaEatsBackend.LogicaNegocio
{
    public class RepartidorService
    {
        private readonly RepartidorDAO _repartidorDAO = new();
        private readonly PedidoDAO _pedidoDAO = new();

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

        public string Actualizar(int id, string cedula, string nombre, string correo, string direccion, string celular, string tarjeta, int amonestaciones)
        {
            var todos = _repartidorDAO.ObtenerTodos();
            var existente = todos.FirstOrDefault(r => r.Id == id);
            if (existente == null)
                return "Error: repartidor no encontrado.";
            if (todos.Any(r => r.Cedula == cedula && r.Id != id))
                return $"Error: cedula {cedula} ya registrada.";
            var rep = new Repartidor(
                id,
                nombre,
                cedula,
                correo,
                direccion,
                celular,
                tarjeta,
                existente.Estado,
                existente.DistanciaDelPedido,
                existente.KilometrosDiarios,
                amonestaciones);
            return _repartidorDAO.ActualizarDatos(rep) ? "Repartidor actualizado." : "Error al actualizar.";
        }

        public string Eliminar(int id)
        {
            if (_pedidoDAO.ContarPorRepartidor(id) > 0)
                return "Error: hay pedidos asociados a este repartidor.";
            return _repartidorDAO.Eliminar(id) ? "Repartidor eliminado." : "Error al eliminar.";
        }
    }
}
