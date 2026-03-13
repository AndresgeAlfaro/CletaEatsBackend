using Microsoft.Data.Sqlite;
using CletaEatsBackend.Modelo;
using CletaEatsBackend.Datos;

namespace CletaEatsBackend.AccesoDatos
{
    public class ComboDAO
    {
        private readonly DatabaseManager _db = DatabaseManager.Instance;

        public bool Insertar(Combo c)
        {
            try
            {
                using var conn = _db.GetConnection(); conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = _db.GetSqlProcedimiento("Combo_Insertar");
                cmd.Parameters.AddWithValue("@idRest", c.IdRestaurante);
                cmd.Parameters.AddWithValue("@num", c.NumeroCombo);
                cmd.Parameters.AddWithValue("@desc", c.Descripcion);
                cmd.Parameters.AddWithValue("@precio", c.Precio);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqliteException) { return false; }
        }

        public List<Combo> ObtenerPorRestaurante(int idRestaurante)
        {
            var lista = new List<Combo>();
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _db.GetSqlProcedimiento("Combo_ObtenerPorRestaurante");
            cmd.Parameters.AddWithValue("@id", idRestaurante);
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Mapear(r));
            return lista;
        }

        public Combo? BuscarPorRestauranteYNumero(int idRestaurante, int numeroCombo)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _db.GetSqlProcedimiento("Combo_BuscarPorRestauranteYNumero");
            cmd.Parameters.AddWithValue("@id", idRestaurante);
            cmd.Parameters.AddWithValue("@num", numeroCombo);
            using var r = cmd.ExecuteReader();
            return r.Read() ? Mapear(r) : null;
        }

        private static Combo Mapear(SqliteDataReader r) => new Combo(
            r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetString(3), r.GetDouble(4));
    }
}
