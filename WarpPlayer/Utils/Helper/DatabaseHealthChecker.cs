using Microsoft.Data.Sqlite;
using Player.Database;
using static Player.Settings.ResourceManifest;


namespace Player.Utils.Helper
{
    public class DatabaseHealthChecker
    {
        public readonly string CreatePlaylistTableSql;
        public readonly string CreateTrackTableSql;
        public readonly string CreatePlaylistTrackTableSql;
        
        private readonly ISqliteConnectionFactory _connectionFactory;
        private readonly DatabaseResources Database;

        public DatabaseHealthChecker(ISqliteConnectionFactory connectionFactory, DatabaseResources database)
        {
            _connectionFactory = connectionFactory;
            Database = database;

            CreatePlaylistTableSql = System.IO.File.ReadAllText(Database.Settings["Playlist"]);
            CreateTrackTableSql = System.IO.File.ReadAllText(Database.Settings["Track"]);
            CreatePlaylistTrackTableSql = System.IO.File.ReadAllText(Database.Settings["PlaylistTrack"]);
        }

        public bool CheckTablesExists()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();

            EnsurePlaylistTableCreated();
            EnsureTrackTableCreated();
            EnsurePlaylistTrackTableCreated();

            return true;
        }

        public bool EnsurePlaylistTableCreated()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();
            ExecuteSql(connection, CreatePlaylistTableSql);

            return true;
        }

        public bool EnsureTrackTableCreated()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();
            ExecuteSql(connection, CreateTrackTableSql);

            return true;
        }

        public bool EnsurePlaylistTrackTableCreated()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();
            ExecuteSql(connection, CreatePlaylistTrackTableSql);

            return true;
        }

        private static void ExecuteSql(SqliteConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }
}
