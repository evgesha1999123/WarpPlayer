using Microsoft.Data.Sqlite;
using Player.Database;
using Player.Schema;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Player.Repo
{
    internal record class PlaylistIdentity
    {
        public Guid PlaylistId { get; init; }
        public long CreatedAt { get; init; }
        public long UpdatedAt { get; init; }
    }

    public class PlaylistRepository : IPlaylistRepository<Playlist, PlaylistRecord>
    {
        private readonly ISqliteConnectionFactory _connectionFactory;
        private readonly object NULL = (object)DBNull.Value;

        public PlaylistRepository(ISqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private PlaylistIdentity GeneratePlaylistIdentity()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return new PlaylistIdentity
            {
                PlaylistId = Guid.NewGuid(),
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        public PlaylistRecord Create(Playlist playlist)
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = GetCreateSql();
            PlaylistIdentity identity = GeneratePlaylistIdentity();
            PutParametersToCommand(playlist, identity, command);

            command.ExecuteNonQuery();

            return BuildPlaylistRecordFrom(playlist, identity);
        }

        private string GetCreateSql()
        {
            return """
                insert into Playlist (PlaylistId, Name, Description, CreatedAt, UpdatedAt)
                values ($PlaylistId, $Name, $Description, $CreatedAt, $UpdatedAt);
                """;
        }

        private void PutParametersToCommand(Playlist playlist, PlaylistIdentity identity, SqliteCommand command)
        {
            command.Parameters.AddWithValue("$PlaylistId", identity.PlaylistId.ToString());
            command.Parameters.AddWithValue("$Name", playlist.Name.ToString());
            command.Parameters.AddWithValue("$Description", playlist?.Description?.ToString() ?? NULL);
            command.Parameters.AddWithValue("$CreatedAt", identity.CreatedAt);
            command.Parameters.AddWithValue("$UpdatedAt", identity.UpdatedAt);
        }

        private PlaylistRecord BuildPlaylistRecordFrom(Playlist playlist, PlaylistIdentity identity)
        {
            return new PlaylistRecord
            {
                PlaylistId = identity.PlaylistId,
                Name = playlist.Name,
                Description = playlist.Description,
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(identity.CreatedAt).UtcDateTime,
                UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(identity.UpdatedAt).UtcDateTime
            };
        }

        public IReadOnlyList<PlaylistRecord> CreateMany(IReadOnlyCollection<Playlist> playlists)
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = GetCreateSql();

            List<PlaylistRecord> result = [];

            foreach(Playlist p in playlists)
            {
                command.Parameters.Clear();

                var identity = GeneratePlaylistIdentity();
                PutParametersToCommand(p, identity, command);

                command.ExecuteNonQuery();
                result.Add(BuildPlaylistRecordFrom(p, identity));
            }

            transaction.Commit();
            return result;
        }

        public bool Delete(Guid id)
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "delete from Playlist where PlaylistId=$PlaylistId;";
            command.Parameters.AddWithValue("$PlaylistId", id.ToString());

            return command.ExecuteNonQuery() == 1;
        }

        public void DeleteAll()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "delete from Playlist;";
            command.ExecuteNonQuery();
        }

        public void DeleteMany(IReadOnlyCollection<Guid> ids)
        {
            using var connection = _connectionFactory.Create();
            
            connection.Open();

            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;

            var parameters = ids.Select((id, i) => new { id, name = $"$id{i}" }).ToList();
            string paramsSql = $"{string.Join(", ", parameters.Select(x => x.name))}";

            command.CommandText = $"delete from Playlist where PlaylistId in ({paramsSql});";

            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.name, param.id.ToString());
            }

            command.ExecuteNonQuery();
            transaction.Commit();
        }

        public PlaylistRecord Get(Guid id)
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = $"select * from Playlist where PlaylistId=$PlaylistId;";
            command.Parameters.AddWithValue("$PlaylistId", id.ToString());

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                throw new KeyNotFoundException($"<{this}> Record with id='{id}' does not exist");
            }

            return GetModelFrom(reader);
        }

        private PlaylistRecord GetModelFrom(DbDataReader reader)
        {
            return new PlaylistRecord
            {
                PlaylistId = Guid.Parse(reader["PlaylistId"].ToString()!),
                Name = reader["Name"].ToString()!,
                Description = reader["Description"] is DBNull ? null : reader["Description"].ToString(),
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(int.Parse(reader["CreatedAt"].ToString()!)).UtcDateTime,
                UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(int.Parse(reader["UpdatedAt"].ToString()!)).UtcDateTime,
            };
        }

        public IReadOnlyList<PlaylistRecord> GetAll()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "select * from Playlist;";
            command.ExecuteNonQuery();

            using var reader = command.ExecuteReader();

            List<PlaylistRecord> result = [];

            while (reader.Read())
            {
                result.Add(GetModelFrom(reader));
            }

            return result;
        }

        public IReadOnlyList<PlaylistRecord> GetMany(IReadOnlyCollection<Guid> ids)
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();

            var parameters = ids.Select((id, i) => new { id, name = $"$id{i}" }).ToList();
            string sqlParams = string.Join(", ", parameters.Select(x => x.name));

            foreach (var parameter in parameters)
            {
                command.Parameters.AddWithValue(parameter.name, parameter.id.ToString());
            }

            command.CommandText = $"select * from Playlist where PlaylistId in ({sqlParams});";
            using var reader = command.ExecuteReader();

            List<PlaylistRecord> result = [];

            while (reader.Read())
            {
                result.Add(GetModelFrom(reader));
            }

            return result;
        }
    }
}
