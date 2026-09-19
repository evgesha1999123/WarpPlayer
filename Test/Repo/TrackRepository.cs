using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.Json;
using Test.Database;
using Test.Schema;


namespace Test.Repo
{
    internal record class TrackIdentity
    {
        public Guid TrackId { get; init; }
        public long CreatedAt { get; init; }
        public long UpdatedAt { get; init; }
    }

    class TrackRepository : ITrackRepository<Track, TrackRecord>
    {
        private readonly ISqliteConnectionFactory _connectionFactory;
        private readonly object NULL = (object)DBNull.Value;
        public TrackRepository(ISqliteConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public TrackRecord Create(Track file)
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = GetCreateSql();

            TrackIdentity trackIdentity = GenerateTrackIdentity();
            PutParametersToCommand(file, trackIdentity, command);
            command.ExecuteNonQuery();

            return BuildTrackRecordFrom(file, trackIdentity);
        }

        private TrackIdentity GenerateTrackIdentity()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return new TrackIdentity
            {
                TrackId = Guid.NewGuid(),
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        private TrackRecord BuildTrackRecordFrom(Track file, TrackIdentity trackIdentity)
        {
            return new TrackRecord
            {
                TrackId = trackIdentity.TrackId,
                Title = file.Title,
                Cover = file.Cover,
                Artist = file.Artist,
                Album = file.Album,
                AlbumArtist = file.AlbumArtist,
                Duration = file.Duration,
                FilePath = file.FilePath,
                Genres = file.Genres,
                Year = file.Year,
                TrackNumber = file.TrackNumber,
                DiscNumber = file.DiscNumber,
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(trackIdentity.CreatedAt).UtcDateTime,
                UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(trackIdentity.UpdatedAt).UtcDateTime,
            };
        }

        private static string GetCreateSql()
        {
            return
                """
                    insert into Track 
                    (
                        TrackId, 
                        Title, 
                        Cover, 
                        Artist, 
                        Album, 
                        AlbumArtist,
                        Duration,
                        FilePath,
                        Genres, 
                        Year, 
                        TrackNumber,
                        DiscNumber,
                        CreatedAt,
                        UpdatedAt
                    )
                    values 
                    (
                        $TrackId, 
                        $Title, 
                        $Cover, 
                        $Artist, 
                        $Album, 
                        $AlbumArtist,
                        $Duration,
                        $FilePath,
                        $Genres, 
                        $Year, 
                        $TrackNumber,
                        $DiscNumber,
                        $CreatedAt,
                        $UpdatedAt
                    );
                    """;
        }

        private void PutParametersToCommand(Track file, TrackIdentity trackIdentity, SqliteCommand command) 
        {
            command.Parameters.AddWithValue("$TrackId", trackIdentity.TrackId.ToString());

            command.Parameters.AddWithValue("$Title", file.Title);

            command.Parameters.AddWithValue("$Cover", file.Cover ?? NULL);

            command.Parameters.AddWithValue("$Artist", file.Artist);

            command.Parameters.AddWithValue("$Album", file.Album ?? NULL);

            command.Parameters.AddWithValue("$AlbumArtist", file.AlbumArtist ?? NULL);

            command.Parameters.AddWithValue("$Duration", (int)file.Duration.TotalSeconds);

            command.Parameters.AddWithValue("$FilePath", file.FilePath);

            command.Parameters.AddWithValue("$Genres", JsonSerializer.Serialize(file.Genres));

            command.Parameters.AddWithValue("$Year", file.Year ?? NULL);

            command.Parameters.AddWithValue("$TrackNumber", file.TrackNumber ?? NULL);

            command.Parameters.AddWithValue("$DiscNumber", file.DiscNumber ?? NULL);

            command.Parameters.AddWithValue("$CreatedAt", trackIdentity.CreatedAt);

            command.Parameters.AddWithValue("$UpdatedAt", trackIdentity.UpdatedAt);
        }

        public IReadOnlyList<TrackRecord> CreateMany(IReadOnlyCollection<Track> files)
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;

            command.CommandText = GetCreateSql();

            List<TrackRecord> result = [];

            foreach (Track file in files)
            {
                command.Parameters.Clear();

                var trackIdentity = GenerateTrackIdentity();
                PutParametersToCommand(file, trackIdentity, command);
                command.ExecuteNonQuery();
                result.Add(BuildTrackRecordFrom(file, trackIdentity));
            }

            transaction.Commit();

            return result;
        }

        public bool Delete(Guid id)
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = "delete from Track where TrackId=$TrackId;";
            command.Parameters.AddWithValue("$TrackId", id.ToString());

            return command.ExecuteNonQuery() == 1;
        }

        public void DeleteAll()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "delete from Track;";
            command.ExecuteNonQuery();
        }

        public void DeleteMany(IReadOnlyCollection<Guid> ids)
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();
            var parameters = ids.Select((id, i) => new { id, name = $"id{i}" }).ToList();
            string params_sql = $"{string.Join(", ", parameters.Select(x => x.name))}";

            command.CommandText = $"delete from Track where TrackId in ({params_sql})";

            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.name, param.id.ToString());
            }

            command.ExecuteNonQuery();
        }

        public TrackRecord Get(Guid id)
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "select * from Track where TrackId=$TrackId;";
            command.Parameters.AddWithValue("$TrackId", id.ToString());

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                throw new KeyNotFoundException($"<{this}> Record with id='{id}' does not exist");
            }

            return GetModelFrom(reader);
        }

        public IReadOnlyList<TrackRecord> GetAll()
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "select * from Track;";

            using var reader = command.ExecuteReader();

            List<TrackRecord> result = [];

            while (reader.Read())
            {
                result.Add(GetModelFrom(reader));
            }

            return result;
        }

        public IReadOnlyList<TrackRecord> GetMany(IReadOnlyCollection<Guid> ids)
        {
            using var connection = _connectionFactory.Create();
            connection.Open();

            using var command = connection.CreateCommand();

            var parameters = ids.Select((id, i) => new { id, name = $"id{i}" }).ToList();
            string sql_params = string.Join(", ", parameters.Select(x => x.name));

            command.CommandText = $"select * from Track where TrackId in ({sql_params})";

            foreach (var parameter in parameters)
            {
                command.Parameters.AddWithValue(parameter.name, parameter.id.ToString());
            }

            List<TrackRecord> result = [];

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(GetModelFrom(reader));
            }

            return result;
        }

        private static TrackRecord GetModelFrom(DbDataReader reader)
        {
            return new TrackRecord
            {
                TrackId = Guid.Parse(reader["TrackId"].ToString()!),

                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(int.Parse(reader["CreatedAt"].ToString()!)).UtcDateTime,

                UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(int.Parse(reader["UpdatedAt"].ToString()!)).UtcDateTime,

                Title = (string)reader["Title"],

                Artist = (string)reader["Artist"],

                FilePath = (string)reader["FilePath"],

                Cover = reader["Cover"] is DBNull ? null : (byte[])reader["Cover"],

                Album = reader["Album"] is DBNull ? null : reader["Album"].ToString(),

                AlbumArtist = reader["AlbumArtist"] is DBNull ? null : reader["AlbumArtist"].ToString(),

                Duration = TimeSpan.FromSeconds(int.Parse(reader["Duration"].ToString()!)),

                Genres = JsonSerializer.Deserialize<string[]>(reader["Genres"].ToString()!)!,

                Year = reader["Year"] is DBNull ? null : int.Parse(reader["Year"].ToString()!),

                TrackNumber = reader["TrackNumber"] is DBNull ? null : int.Parse(reader["TrackNumber"].ToString()!),

                DiscNumber = reader["DiscNumber"] is DBNull ? null : int.Parse(reader["DiscNumber"].ToString()!),
            };
        }
    }
}
