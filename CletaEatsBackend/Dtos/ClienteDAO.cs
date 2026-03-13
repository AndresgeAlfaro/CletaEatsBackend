using Microsoft.Data.Sqlite;
using CletaEatsBackend.Modelo;
using CletaEatsBackend.Datos;

namespace CletaEatsBackend.AccesoDatos
{
    public class ClienteDAO
    {
        private readonly DatabaseManager _db = DatabaseManager.Instance;

        public bool Insertar(Cliente c)
        {
            try
            {
                using var conn = _db.GetConnection(); conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = _db.GetSqlProcedimiento("Cliente_Insertar");
                cmd.Parameters.AddWithValue("@ced", c.Cedula);
                cmd.Parameters.AddWithValue("@nom", c.Nombre);
                cmd.Parameters.AddWithValue("@dir", c.DireccionExacta);
                cmd.Parameters.AddWithValue("@tar", c.NumeroTarjeta);
                cmd.Parameters.AddWithValue("@cel", c.NumeroCelular);
                cmd.Parameters.AddWithValue("@cor", c.CorreoElectronico);
                cmd.Parameters.AddWithValue("@est", c.Estado.ToString());
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqliteException) { return false; }
        }

        public List<Cliente> ObtenerTodos()
        {
            var lista = new List<Cliente>();
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _db.GetSqlProcedimiento("Cliente_ObtenerTodos");
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Mapear(r));
            return lista;
        }

        public Cliente? BuscarPorCedula(string cedula)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _db.GetSqlProcedimiento("Cliente_BuscarPorCedula");
            cmd.Parameters.AddWithValue("@ced", cedula);
            using var r = cmd.ExecuteReader();
            return r.Read() ? Mapear(r) : null;
        }

        public List<Cliente> ObtenerActivos() =>
            ObtenerTodos().FindAll(c => c.Estado == EstadoCliente.ACTIVO);

        public List<Cliente> ObtenerSuspendidos() =>
            ObtenerTodos().FindAll(c => c.Estado == EstadoCliente.SUSPENDIDO);

        public void ActualizarEstado(int id, EstadoCliente estado)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _db.GetSqlProcedimiento("Cliente_ActualizarEstado");
            cmd.Parameters.AddWithValue("@est", estado.ToString());
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private static Cliente Mapear(SqliteDataReader r) => new Cliente(
            r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3),
            r.GetString(4), r.GetString(5), r.GetString(6),
            Enum.Parse<EstadoCliente>(r.GetString(7)));
    }
}
