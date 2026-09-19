

using Test.Database;

namespace Test.Utils
{
    public class DatabaseHealthChecker
    {
        private readonly ISqliteConnectionFactory _connectionFactory;

        public DatabaseHealthChecker(ISqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public bool CheckTablesExists()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();
            return true;
        }
    }
}
