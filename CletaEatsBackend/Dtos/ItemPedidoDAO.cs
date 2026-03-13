using Microsoft.Data.Sqlite;
using CletaEatsBackend.Modelo;
using CletaEatsBackend.Datos;

namespace CletaEatsBackend.AccesoDatos
{
    public class ItemPedidoDAO
    {
        private readonly DatabaseManager _db = DatabaseManager.Instance;

        public void Insertar(ItemPedido item)
        {
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _db.GetSqlProcedimiento("ItemPedido_Insertar");
            cmd.Parameters.AddWithValue("@idPed", item.IdPedido);
            cmd.Parameters.AddWithValue("@num", item.NumeroCombo);
            cmd.Parameters.AddWithValue("@desc", item.Descripcion);
            cmd.Parameters.AddWithValue("@precio", item.PrecioUnitario);
            cmd.Parameters.AddWithValue("@cant", item.Cantidad);
            cmd.ExecuteNonQuery();
        }

        public List<ItemPedido> ObtenerPorPedido(int idPedido)
        {
            var lista = new List<ItemPedido>();
            using var conn = _db.GetConnection(); conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = _db.GetSqlProcedimiento("ItemPedido_ObtenerPorPedido");
            cmd.Parameters.AddWithValue("@id", idPedido);
            using var r = cmd.ExecuteReader();
            while (r.Read()) lista.Add(Mapear(r));
            return lista;
        }

        private static ItemPedido Mapear(SqliteDataReader r) => new ItemPedido(
            r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetString(3), r.GetDouble(4), r.GetInt32(5));
    }
}
