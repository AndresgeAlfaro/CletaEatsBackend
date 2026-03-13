using Microsoft.Data.Sqlite;
using CletaEatsBackend.Modelo;
using CletaEatsBackend.Datos;

namespace CletaEatsBackend.AccesoDatos
{
    public class PedidoDAO
    {
        private readonly DatabaseManager _db = DatabaseManager.Instance;

        public int Insertar(Pedido p)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = _db.GetSqlProcedimiento("Pedido_Insertar");
                cmd.Parameters.AddWithValue("@cli", p.IdCliente);
                cmd.Parameters.AddWithValue("@rest", p.IdRestaurante);
                cmd.Parameters.AddWithValue("@rep", p.IdRepartidor);
                cmd.Parameters.AddWithValue("@hora", p.HoraRealizacion);
                cmd.Parameters.AddWithValue("@est", p.Estado.ToString());
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = _db.GetSqlProcedimiento("LastInsertRowId");
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void ActualizarEstado(int id, EstadoPedido estado, string? horaEntrega = null)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = horaEntrega != null
                ? _db.GetSqlProcedimiento("Pedido_ActualizarEstadoConHora")
                : _db.GetSqlProcedimiento("Pedido_ActualizarEstado");
            cmd.Parameters.AddWithValue("@est", estado.ToString());
            cmd.Parameters.AddWithValue("@id", id);
            if (horaEntrega != null)
                cmd.Parameters.AddWithValue("@hora", horaEntrega);
            cmd.ExecuteNonQuery();
        }

        public List<Pedido> ObtenerTodos()
        {
            var lista = new List<Pedido>();
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _db.GetSqlProcedimiento("Pedido_ObtenerTodos");
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Mapear(r));
            return lista;
        }

        public List<Pedido> ObtenerPorCliente(int idCliente)
        {
            var lista = new List<Pedido>();
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _db.GetSqlProcedimiento("Pedido_ObtenerPorCliente");
            cmd.Parameters.AddWithValue("@id", idCliente);
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Mapear(r));
            return lista;
        }

        public Pedido? BuscarPorId(int id)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _db.GetSqlProcedimiento("Pedido_BuscarPorId");
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? Mapear(r) : null;
        }

        private static Pedido Mapear(SqliteDataReader r) => new Pedido(
            r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetInt32(3),
            r.GetString(4), r.IsDBNull(5) ? null : r.GetString(5),
            Enum.Parse<EstadoPedido>(r.GetString(6)));
    }
}
