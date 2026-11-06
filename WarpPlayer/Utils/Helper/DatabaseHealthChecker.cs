using Microsoft.Data.Sqlite;
using Player.Database;
using Player.Schema;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.DirectoryServices;
using static Player.Settings.ResourceManifest;


namespace Player.Utils.Helper
{
    public class DatabaseHealthChecker
    {   
        private readonly ISqliteConnectionFactory _connectionFactory;
        private readonly DatabaseResources Database;
        private readonly OrderedDictionary QueryDictionary = new(); 

        public DatabaseHealthChecker(ISqliteConnectionFactory connectionFactory, DatabaseResources database)
        {
            _connectionFactory = connectionFactory;
            Database = database;

            QueryDictionary["Playlist"] = System.IO.File.ReadAllText(Database.Settings["Playlist"]);
            QueryDictionary["Track"] = System.IO.File.ReadAllText(Database.Settings["Track"]);
            QueryDictionary["PlaylistTrack"] = System.IO.File.ReadAllText(Database.Settings["PlaylistTrack"]);
        }

        public bool CheckHealth()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            foreach (var query in QueryDictionary.Values)
            {
                ExecuteSql(connection, query.ToString()!);
            }

            using var command = connection.CreateCommand();
            command.CommandText = "select * from Playlist order by CreatedAt desc limit 1;";

            var reader = command.ExecuteReader();
            PlaylistRecord? result = null;

            if (reader.Read())
            {
                result = reader.ToPlaylistRecord();
            }

            reader.Close();

            if (result == null)
            {
                command.CommandText = $"insert into Playlist (PlaylistId, Name, Description, CreatedAt, UpdatedAt)" +
                    $" values ($PlaylistId, $Name, $Description, $CreatedAt, $UpdatedAt);";
                command.Parameters.AddWithValue("$PlaylistId", Guid.NewGuid().ToString());
                command.Parameters.AddWithValue("$Name", "MyPlaylist");
                command.Parameters.AddWithValue("$Description", "System required playlist");
                command.Parameters.AddWithValue("$CreatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                command.Parameters.AddWithValue("$UpdatedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                command.ExecuteNonQuery();
            }
            else
            {
                Debug.WriteLine("Found system playlist! Create skipped.");
            }

            return true;
        }

        public bool EnsurePlaylistTableCreated()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();
            ExecuteSql(connection, QueryDictionary["Playlist"]?.ToString()!);

            return true;
        }

        public bool EnsureTrackTableCreated()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();
            ExecuteSql(connection, QueryDictionary["Track"]?.ToString()!);

            return true;
        }

        public bool EnsurePlaylistTrackTableCreated()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();
            ExecuteSql(connection, QueryDictionary["PlaylistTrack"]?.ToString()!);

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
