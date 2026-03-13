using Microsoft.Data.Sqlite;
using System.IO;

namespace CletaEatsBackend.Datos
{
    // Patron Singleton: garantiza una sola instancia de la conexion
    public class DatabaseManager
    {
        private static DatabaseManager? _instance;
        private static readonly object _lock = new object();
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
        }
    }
}
