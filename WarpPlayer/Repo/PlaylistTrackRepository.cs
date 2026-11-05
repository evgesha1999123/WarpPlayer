using Microsoft.Data.Sqlite;
using Player.Database;
using Player.Schema;
using System;
using System.Collections.Generic;


namespace Player.Repo
{
    public class PlaylistTrackRepository
    {
        ISqliteConnectionFactory connectionFactory;
        public PlaylistTrackRepository(ISqliteConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public PlaylistTrack AddOneTrackToPlaylist(Guid playlistId, Guid trackId)
        {
            using var connection = connectionFactory.Create();
            connection.Open();
            var command = connection.CreateCommand();

            command.CommandText = GetCreateSql();
            PutParametersToCommand(playlistId, trackId, command);

            int insertedRows = command.ExecuteNonQuery();

            if (insertedRows == 1)
            {
                return new PlaylistTrack { PlaylistId = playlistId, TrackId = trackId };
            }
            throw new InvalidOperationException($"Запись PlaylistTrack с PlaylistId={playlistId}, TrackId={trackId} не была создана.");
        }

        public IReadOnlyList<PlaylistTrack> AddManyTracksToPlaylist(Guid playlistId, IReadOnlyCollection<Guid> trackIds)
        {
            using var connection = connectionFactory.Create();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            var command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText = GetCreateSql();

            List<PlaylistTrack> result = [];

            foreach (var id in trackIds)
            {
                command.Parameters.Clear();
                PutParametersToCommand(playlistId, id, command);
                int newRows = command.ExecuteNonQuery();
                if (newRows == 1)
                {
                    result.Add(new PlaylistTrack { PlaylistId = playlistId, TrackId = id });
                }
                else
                {
                    throw new InvalidOperationException($"Запись PlaylistTrack с PlaylistId={playlistId}, TrackId={id} не была создана.");
                }
            }
            transaction.Commit();
            return result;
        }

        public IReadOnlyList<PlaylistTrack> GetPlaylistById(Guid playlistId)
        {
            using var connection = connectionFactory.Create();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "select * from PlaylistTrack where PlaylistId=$PlaylistId;";
            command.Parameters.AddWithValue("$PlaylistId", playlistId.ToString());

            using var reader = command.ExecuteReader();

            List<PlaylistTrack> result = [];

            while (reader.Read())
            {
                result.Add(new PlaylistTrack { PlaylistId = Guid.Parse(reader["PlaylistId"].ToString()!), TrackId = Guid.Parse(reader["TrackId"].ToString()!) });
            }

            return result;
        }

        public IReadOnlyList<PlaylistTrack> GetAllPlaylists()
        {
            using var connection = connectionFactory.Create();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "select * from PlaylistTrack;";

            using var reader = command.ExecuteReader();

            List<PlaylistTrack> result = [];

            while (reader.Read())
            {
                result.Add(new PlaylistTrack { PlaylistId = Guid.Parse(reader["PlaylistId"].ToString()!), TrackId = Guid.Parse(reader["TrackId"].ToString()!) });
            }

            return result;
        }

        public void DeletePlaylistById(Guid playlistId)
        {
            using var connection = connectionFactory.Create();
            connection.Open();
            using var command = connection.CreateCommand();

            command.CommandText = "delete from PlaylistTrack where PlaylistId=$PlaylistId;";
            command.Parameters.AddWithValue("$PlaylistId", playlistId.ToString());

            int rowsDeleted = command.ExecuteNonQuery();

            if (rowsDeleted == 0)
            {
                throw new InvalidOperationException($"Не удалось удалить плейлист с id={playlistId}");
            }
        }

        private static string GetCreateSql()
        {
            return "insert into PlaylistTrack(PlaylistId, TrackId) values ($PlaylistId, $TrackId);";
        }

        private static void PutParametersToCommand(Guid playlistId, Guid trackId, SqliteCommand command)
        {
            command.Parameters.AddWithValue($"PlaylistId", playlistId.ToString());
            command.Parameters.AddWithValue($"TrackId", trackId.ToString());
        }
    }
}
