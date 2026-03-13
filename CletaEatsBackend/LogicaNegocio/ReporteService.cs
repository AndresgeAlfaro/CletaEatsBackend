using CletaEatsBackend.Datos;
using Microsoft.Data.Sqlite;

namespace CletaEatsBackend.LogicaNegocio
{
    /// <summary>
    /// Servicio de reportes que consulta vistas y tablas para los reportes i-p.
    /// </summary>
    public class ReporteService
    {
        public string RestauranteConMasPedidos()
        {
            using var conn = DatabaseManager.Instance.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT nombreRestaurante, totalPedidos
FROM vw_PedidosPorRestaurante ORDER BY totalPedidos DESC LIMIT 1";
            using var r = cmd.ExecuteReader();
            return r.Read()
                ? $"Restaurante con mas pedidos: {r.GetString(0)} ({r.GetInt64(1)} pedidos)"
                : "Sin datos.";
        }

        public List<(string nombre, double monto)> MontoPorRestaurante()
        {
            var lista = new List<(string, double)>();
            using var conn = DatabaseManager.Instance.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT nombreRestaurante, montoTotal FROM vw_PedidosPorRestaurante";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                lista.Add((r.GetString(0), r.GetDouble(1)));
            return lista;
        }

        public double MontoTotalGeneral()
        {
            using var conn = DatabaseManager.Instance.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COALESCE(SUM(total), 0) FROM Factura";
            return Convert.ToDouble(cmd.ExecuteScalar());
        }

        public string RestauranteConMenosPedidos()
        {
            using var conn = DatabaseManager.Instance.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT nombreRestaurante, totalPedidos
FROM vw_PedidosPorRestaurante ORDER BY totalPedidos ASC LIMIT 1";
            using var r = cmd.ExecuteReader();
            return r.Read()
                ? $"Restaurante con menos pedidos: {r.GetString(0)} ({r.GetInt64(1)} pedidos)"
                : "Sin datos.";
        }

        public List<string> QuejasPorRepartidor()
        {
            var lineas = new List<string>();
            using var conn = DatabaseManager.Instance.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT rp.nombre, rp.cedula, q.id, q.fecha, q.categoria, q.descripcion
FROM Repartidor rp
LEFT JOIN Queja q ON q.idRepartidor = rp.id
ORDER BY rp.nombre, q.fecha";
            using var r = cmd.ExecuteReader();
            string repActual = "";
            while (r.Read())
            {
                string repNombre = r.GetString(0);
                if (repNombre != repActual)
                {
                    repActual = repNombre;
                    lineas.Add($"\nRepartidor: {repNombre} (Cedula: {r.GetString(1)})");
                }
                if (r.IsDBNull(2))
                    lineas.Add("  Sin quejas registradas.");
                else
                    lineas.Add($"  [{r.GetString(4)}] {r.GetString(3)} - {r.GetString(5)}");
            }
            return lineas;
        }

        public List<string> PedidosPorCliente()
        {
            var lineas = new List<string>();
            using var conn = DatabaseManager.Instance.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT c.nombre, c.cedula, p.id, p.horaRealizacion, p.estado
FROM Cliente c
LEFT JOIN Pedido p ON p.idCliente = c.id
ORDER BY c.nombre, p.horaRealizacion";
            using var r = cmd.ExecuteReader();
            string cliActual = "";
            while (r.Read())
            {
                string cliNombre = r.GetString(0);
                if (cliNombre != cliActual)
                {
                    cliActual = cliNombre;
                    lineas.Add($"\nCliente: {cliNombre} (Cedula: {r.GetString(1)})");
                }
                if (r.IsDBNull(2))
                    lineas.Add("  Sin pedidos registrados.");
                else
                    lineas.Add($"  Pedido #{r.GetInt32(2)} | {r.GetString(3)} | Estado: {r.GetString(4)}");
            }
            return lineas;
        }

        public string ClienteConMasPedidos()
        {
            using var conn = DatabaseManager.Instance.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT nombreCliente, cedula, totalPedidos
FROM vw_PedidosPorCliente ORDER BY totalPedidos DESC LIMIT 1";
            using var r = cmd.ExecuteReader();
            return r.Read()
                ? $"Cliente con mas pedidos: {r.GetString(0)} (Cedula: {r.GetString(1)}) con {r.GetInt64(2)} pedidos."
                : "Sin datos.";
        }

        public string HoraPico()
        {
            using var conn = DatabaseManager.Instance.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT SUBSTR(horaRealizacion, 12, 2) AS hora, COUNT(*) AS cnt
FROM Pedido GROUP BY hora ORDER BY cnt DESC LIMIT 1";
            using var r = cmd.ExecuteReader();
            return r.Read()
                ? $"Hora pico: {r.GetString(0)}:00 hrs ({r.GetInt64(1)} pedidos)"
                : "Sin datos.";
        }
    }
}
