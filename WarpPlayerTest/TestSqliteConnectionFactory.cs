using Microsoft.Data.Sqlite;
using Player.Database;


namespace WarpPlayerTest
{
    internal sealed class TestSqliteConnectionFactory
    : ISqliteConnectionFactory
    {
        private readonly string _connectionString;

        public TestSqliteConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqliteConnection Create()
        {
            return new SqliteConnection(_connectionString);
        }
    }
}
