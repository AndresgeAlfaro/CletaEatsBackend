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
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Pedido
(idCliente, idRestaurante, idRepartidor, horaRealizacion, estado)
VALUES (@cli, @rest, @rep, @hora, @est);
SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@cli", p.IdCliente);
            cmd.Parameters.AddWithValue("@rest", p.IdRestaurante);
            cmd.Parameters.AddWithValue("@rep", p.IdRepartidor);
            cmd.Parameters.AddWithValue("@hora", p.HoraRealizacion);
            cmd.Parameters.AddWithValue("@est", p.Estado.ToString());
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void ActualizarEstado(int id, EstadoPedido estado, string? horaEntrega = null)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            if (horaEntrega != null)
            {
                cmd.CommandText = @"UPDATE Pedido SET estado = @est, horaEntrega = @hora WHERE id = @id";
                cmd.Parameters.AddWithValue("@hora", horaEntrega);
            }
            else
            {
                cmd.CommandText = "UPDATE Pedido SET estado = @est WHERE id = @id";
            }
            cmd.Parameters.AddWithValue("@est", estado.ToString());
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public List<Pedido> ObtenerTodos()
        {
            var lista = new List<Pedido>();
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Pedido";
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Mapear(r));
            return lista;
        }

        public List<Pedido> ObtenerPorCliente(int idCliente)
        {
            var lista = new List<Pedido>();
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Pedido WHERE idCliente = @id";
            cmd.Parameters.AddWithValue("@id", idCliente);
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Mapear(r));
            return lista;
        }

        public Pedido? BuscarPorId(int id)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Pedido WHERE id = @id";
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
