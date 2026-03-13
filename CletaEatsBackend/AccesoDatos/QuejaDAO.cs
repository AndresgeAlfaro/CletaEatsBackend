using Microsoft.Data.Sqlite;
using CletaEatsBackend.Modelo;
using CletaEatsBackend.Datos;

namespace CletaEatsBackend.AccesoDatos
{
    public class QuejaDAO
    {
        private readonly DatabaseManager _db = DatabaseManager.Instance;

        public bool Insertar(Queja q)
        {
            try
            {
                using var conn = _db.GetConnection(); conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Queja (idRepartidor, idPedido, idCliente, descripcion, fecha, categoria)
VALUES (@idRep, @idPed, @idCli, @desc, @fecha, @cat)";
                cmd.Parameters.AddWithValue("@idRep", q.IdRepartidor);
                cmd.Parameters.AddWithValue("@idPed", q.IdPedido);
                cmd.Parameters.AddWithValue("@idCli", q.IdCliente);
                cmd.Parameters.AddWithValue("@desc", q.Descripcion);
                cmd.Parameters.AddWithValue("@fecha", q.Fecha);
                cmd.Parameters.AddWithValue("@cat", q.Categoria.ToString());
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqliteException) { return false; }
        }

        public List<Queja> ObtenerTodas()
        {
            var lista = new List<Queja>();
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Queja ORDER BY fecha";
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Mapear(r));
            return lista;
        }

        public List<Queja> ObtenerPorRepartidor(int idRepartidor)
        {
            var lista = new List<Queja>();
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Queja WHERE idRepartidor = @id ORDER BY fecha";
            cmd.Parameters.AddWithValue("@id", idRepartidor);
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Mapear(r));
            return lista;
        }

        private static Queja Mapear(SqliteDataReader r) => new Queja(
            r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetInt32(3),
            r.GetString(4), r.GetString(5), Enum.Parse<CategoriaQueja>(r.GetString(6)));
    }
}
