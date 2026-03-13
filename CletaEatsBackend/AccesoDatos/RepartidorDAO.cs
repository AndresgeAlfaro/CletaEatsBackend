using Microsoft.Data.Sqlite;
using CletaEatsBackend.Modelo;
using CletaEatsBackend.Datos;

namespace CletaEatsBackend.AccesoDatos
{
    public class RepartidorDAO
    {
        private readonly DatabaseManager _db = DatabaseManager.Instance;

        public bool Insertar(Repartidor r)
        {
            try
            {
                using var conn = _db.GetConnection(); conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO Repartidor
(nombre, cedula, correo, direccion, celular, tarjeta, estado, distanciaPedido, kmDiarios, amonestaciones)
VALUES (@nom, @ced, @cor, @dir, @cel, @tar, @est, @dist, @km, @amon)";
                cmd.Parameters.AddWithValue("@nom", r.Nombre);
                cmd.Parameters.AddWithValue("@ced", r.Cedula);
                cmd.Parameters.AddWithValue("@cor", r.CorreoElectronico);
                cmd.Parameters.AddWithValue("@dir", r.DireccionExacta);
                cmd.Parameters.AddWithValue("@cel", r.NumeroCelular);
                cmd.Parameters.AddWithValue("@tar", r.NumeroTarjeta);
                cmd.Parameters.AddWithValue("@est", r.Estado.ToString());
                cmd.Parameters.AddWithValue("@dist", r.DistanciaDelPedido);
                cmd.Parameters.AddWithValue("@km", r.KilometrosDiarios);
                cmd.Parameters.AddWithValue("@amon", r.NumeroAmonestaciones);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqliteException) { return false; }
        }

        public List<Repartidor> ObtenerTodos()
        {
            var lista = new List<Repartidor>();
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Repartidor";
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Mapear(r));
            return lista;
        }

        public Repartidor? ObtenerPrimerDisponible()
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Repartidor
WHERE estado = 'DISPONIBLE' AND amonestaciones < 4
ORDER BY id LIMIT 1";
            using var r = cmd.ExecuteReader();
            return r.Read() ? Mapear(r) : null;
        }

        public void ActualizarEstado(int id, EstadoRepartidor estado)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Repartidor SET estado = @est WHERE id = @id";
            cmd.Parameters.AddWithValue("@est", estado.ToString());
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void IncrementarAmonestacion(int id)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Repartidor
SET amonestaciones = amonestaciones + 1,
estado = CASE WHEN amonestaciones + 1 >= 4 THEN 'EXPULSADO' ELSE estado END
WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private static Repartidor Mapear(SqliteDataReader r) => new Repartidor(
            r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetString(3),
            r.GetString(4), r.GetString(5), r.GetString(6),
            Enum.Parse<EstadoRepartidor>(r.GetString(7)),
            r.GetDouble(8), r.GetDouble(9), r.GetInt32(10));
    }
}
