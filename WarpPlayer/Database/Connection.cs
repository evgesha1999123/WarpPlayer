using Microsoft.Data.Sqlite;


namespace Player.Database
{
    public interface ISqliteConnectionFactory
    {
        SqliteConnection Create();
    }

    public class SqliteConnectionFactory : ISqliteConnectionFactory
    {
        private readonly string _connectionString;

        public SqliteConnectionFactory(string databasePath)
        {
            _connectionString = $"Data Source={databasePath}";
        }
        public SqliteConnection Create()
        {
            return new SqliteConnection(_connectionString);
        }
    }
}
