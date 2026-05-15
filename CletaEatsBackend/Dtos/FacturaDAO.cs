using Microsoft.Data.Sqlite;
using CletaEatsBackend.Modelo;
using CletaEatsBackend.Datos;

namespace CletaEatsBackend.AccesoDatos
{
    public class FacturaDAO
    {
        private readonly DatabaseManager _db = DatabaseManager.Instance;

        public bool Insertar(Factura f)
        {
            try
            {
                using var conn = _db.GetConnection(); conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = _db.GetSqlProcedimiento("Factura_Insertar");
                cmd.Parameters.AddWithValue("@idPed", f.IdPedido);
                cmd.Parameters.AddWithValue("@sub", f.Subtotal);
                cmd.Parameters.AddWithValue("@trans", f.CostoTransporte);
                cmd.Parameters.AddWithValue("@iva", f.Iva);
                cmd.Parameters.AddWithValue("@total", f.Total);
                cmd.Parameters.AddWithValue("@fecha", f.FechaEmision);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (SqliteException) { return false; }
        }

        public Factura? BuscarPorPedido(int idPedido)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _db.GetSqlProcedimiento("Factura_BuscarPorPedido");
            cmd.Parameters.AddWithValue("@id", idPedido);
            using var r = cmd.ExecuteReader();
            return r.Read() ? Mapear(r) : null;
        }

        public void EliminarPorPedido(int idPedido)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _db.GetSqlProcedimiento("Factura_EliminarPorPedido");
            cmd.Parameters.AddWithValue("@id", idPedido);
            cmd.ExecuteNonQuery();
        }

        private static Factura Mapear(SqliteDataReader r)
        {
            var f = new Factura
            {
                Id = r.GetInt32(0),
                IdPedido = r.GetInt32(1),
                Subtotal = r.GetDouble(2),
                CostoTransporte = r.GetDouble(3),
                Iva = r.GetDouble(4),
                Total = r.GetDouble(5),
                FechaEmision = r.GetString(6)
            };
            return f;
        }
    }
}
