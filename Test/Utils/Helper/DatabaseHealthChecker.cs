using Test.Database;

namespace Test.Utils.Helper
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

            string playlistTableSql = System.IO.File.ReadAllText(@"C:\Users\evges\source\repos\Test\Test\Database\Helper\Query\CreateIfNotExists\PlaylistTable.sql");
            string playlistTrackTableSql = System.IO.File.ReadAllText(@"C:\Users\evges\source\repos\Test\Test\Database\Helper\Query\CreateIfNotExists\PlaylistTrackTable.sql");
            string trackTableSql = System.IO.File.ReadAllText(@"C:\Users\evges\source\repos\Test\Test\Database\Helper\Query\CreateIfNotExists\TrackTable.sql");

            using var command = connection.CreateCommand();

            command.CommandText = playlistTableSql;
            command.ExecuteNonQuery();

            command.CommandText = playlistTrackTableSql;
            command.ExecuteNonQuery();

            command.CommandText = trackTableSql;
            command.ExecuteNonQuery();

            return true;
        }
    }
}
