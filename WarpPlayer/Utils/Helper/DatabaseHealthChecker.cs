using System;
using System.Diagnostics;
using Player.Database;
using static Player.Settings.ResourceManifest;


namespace Player.Utils.Helper
{
    public class DatabaseHealthChecker
    {
        private readonly ISqliteConnectionFactory _connectionFactory;
        private readonly DatabaseResources Database;

        public DatabaseHealthChecker(ISqliteConnectionFactory connectionFactory, DatabaseResources database)
        {
            _connectionFactory = connectionFactory;
            Database = database;
        }

        public bool CheckTablesExists()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            string playlistSql = System.IO.File.ReadAllText(Database.Settings["Playlist"]);
            string playlistTrackSql = System.IO.File.ReadAllText(Database.Settings["PlaylistTrack"]);
            string trackSql = System.IO.File.ReadAllText(Database.Settings["Track"]);

            using var command = connection.CreateCommand();

            command.CommandText = playlistSql;
            command.ExecuteNonQuery();

            command.CommandText = playlistTrackSql;
            command.ExecuteNonQuery();

            command.CommandText = trackSql;
            command.ExecuteNonQuery();

            return true;
        }
    }
}
