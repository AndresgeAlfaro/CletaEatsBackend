using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;

namespace CletaEatsBackend.Datos
{
    // Patron Singleton: garantiza una sola instancia de la conexion
    public class DatabaseManager
    {
        private static DatabaseManager? _instance;
        private static readonly object _lock = new object();
        private static Dictionary<string, string>? _procedimientosCache;
        private const string DbPath = "cletaeats.db";

        public static DatabaseManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null) _instance = new DatabaseManager();
                    return _instance;
                }
            }
        }

        private DatabaseManager() { InicializarBD(); }

        public SqliteConnection GetConnection() =>
            new SqliteConnection($"Data Source={DbPath}");

        /// <summary>
        /// Obtiene el texto SQL del procedimiento almacenado por nombre.
        /// Los DAOs solo deben ejecutar SQL obtenido mediante este metodo.
        /// </summary>
        public string GetSqlProcedimiento(string nombre)
        {
            if (_procedimientosCache != null && _procedimientosCache.TryGetValue(nombre, out string? sql))
                return sql;
            lock (_lock)
            {
                _procedimientosCache ??= new Dictionary<string, string>();
                if (_procedimientosCache.TryGetValue(nombre, out sql))
                    return sql;
                using var conn = GetConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT SqlTexto FROM ProcedimientoAlmacenado WHERE Nombre = @nom";
                cmd.Parameters.AddWithValue("@nom", nombre);
                var obj = cmd.ExecuteScalar();
                if (obj == null || obj == DBNull.Value)
                    throw new InvalidOperationException($"Procedimiento almacenado no encontrado: {nombre}");
                sql = (string)obj;
                _procedimientosCache[nombre] = sql;
                return sql;
            }
        }

        private void InicializarBD()
        {
            using var conn = GetConnection();
            conn.Open();
            var schemaPath = Path.Combine(AppContext.BaseDirectory, "Datos", "schema.sql");
            if (!File.Exists(schemaPath))
                schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "Datos", "schema.sql");
            if (!File.Exists(schemaPath))
                schemaPath = "schema.sql";
            string schema = File.ReadAllText(schemaPath);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = schema;
            cmd.ExecuteNonQuery();
            EnsurePedidoObservacionColumn(conn);
            _procedimientosCache = null;
        }

        private static void EnsurePedidoObservacionColumn(SqliteConnection conn)
        {
            var cols = new List<string>();
            using (var pragma = conn.CreateCommand())
            {
                pragma.CommandText = "PRAGMA table_info(Pedido)";
                using var r = pragma.ExecuteReader();
                while (r.Read())
                    cols.Add(r.GetString(1));
            }
            if (cols.Exists(c => string.Equals(c, "observacion", StringComparison.OrdinalIgnoreCase)))
                return;
            using var alter = conn.CreateCommand();
            alter.CommandText = "ALTER TABLE Pedido ADD COLUMN observacion TEXT NOT NULL DEFAULT ''";
            alter.ExecuteNonQuery();
        }
    }
}
