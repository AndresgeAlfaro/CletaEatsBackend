using CletaEatsBackend.AccesoDatos;
using CletaEatsBackend.Modelo;

namespace CletaEatsBackend.LogicaNegocio
{
    public class ClienteService
    {
        private readonly ClienteDAO _clienteDAO = new();

        public string Registrar(string cedula, string nombre, string direccion, string tarjeta, string celular, string correo)
        {
            if (_clienteDAO.BuscarPorCedula(cedula) != null)
                return $"Error: cedula {cedula} ya registrada.";
            var c = new Cliente
            {
                Cedula = cedula,
                Nombre = nombre,
                DireccionExacta = direccion,
                NumeroTarjeta = tarjeta,
                NumeroCelular = celular,
                CorreoElectronico = correo,
                Estado = EstadoCliente.ACTIVO
            };
            return _clienteDAO.Insertar(c)
                ? $"Cliente '{nombre}' registrado."
                : "Error al guardar.";
        }

        public string VerificarAcceso(string cedula)
        {
            var c = _clienteDAO.BuscarPorCedula(cedula);
            if (c == null) return "NO_REGISTRADO";
            return c.Estado == EstadoCliente.ACTIVO ? "ACTIVO" : "SUSPENDIDO";
        }

        public List<Cliente> ObtenerActivos() => _clienteDAO.ObtenerActivos();
        public List<Cliente> ObtenerSuspendidos() => _clienteDAO.ObtenerSuspendidos();
        public Cliente? BuscarPorCedula(string cedula) => _clienteDAO.BuscarPorCedula(cedula);
    }
}
