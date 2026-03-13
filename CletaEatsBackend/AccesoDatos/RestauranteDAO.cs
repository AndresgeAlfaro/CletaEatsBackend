using Microsoft.Data.Sqlite;
using CletaEatsBackend.Modelo;
using CletaEatsBackend.Datos;

namespace CletaEatsBackend.AccesoDatos
{
    public class RestauranteDAO
    {
        private readonly DatabaseManager _db = DatabaseManager.Instance;

        public bool Insertar(Restaurante r)
        {
            try
            {
                using var conn = _db.GetConnection(); conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Restaurante (nombre, cedulaJuridica, direccion, tipoComida)
VALUES (@nom, @ced, @dir, @tipo)";
                cmd.Parameters.AddWithValue("@nom", r.Nombre);
                cmd.Parameters.AddWithValue("@ced", r.CedulaJuridica);
                cmd.Parameters.AddWithValue("@dir", r.Direccion);
                cmd.Parameters.AddWithValue("@tipo", r.TipoComida.ToString());
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqliteException) { return false; }
        }

        public List<Restaurante> ObtenerTodos()
        {
            var lista = new List<Restaurante>();
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Restaurante ORDER BY nombre";
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Mapear(r));
            return lista;
        }

        public Restaurante? BuscarPorId(int id)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Restaurante WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? Mapear(r) : null;
        }

        public Restaurante? BuscarPorCedulaJuridica(string cedulaJuridica)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Restaurante WHERE cedulaJuridica = @ced";
            cmd.Parameters.AddWithValue("@ced", cedulaJuridica);
            using var r = cmd.ExecuteReader();
            return r.Read() ? Mapear(r) : null;
        }

        private static Restaurante Mapear(SqliteDataReader r) => new Restaurante(
            r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3),
            Enum.Parse<TipoComida>(r.GetString(4)));
    }
}
