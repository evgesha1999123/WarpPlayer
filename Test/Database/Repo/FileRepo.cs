using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Test.Schema;
using Windows.Devices.PointOfService;

namespace Test.Database.Repo
{
    class FileRepo : IFileRepository<Track, TrackRecord>
    {
        private readonly SqliteConnection _connection;
        public FileRepo(string sourceFile)
        {
            _connection = new SqliteConnection($"Data Source={sourceFile}");
        }
        public TrackRecord Create(Track file)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = """
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

            var NULL = (object)DBNull.Value;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Guid guid = Guid.NewGuid();

            command.Parameters.AddWithValue("$TrackId", guid.ToString());

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

            command.Parameters.AddWithValue("$CreatedAt", now);

            command.Parameters.AddWithValue("$UpdatedAt", now);

            command.ExecuteNonQuery();

            return new TrackRecord
            {
                TrackId = guid,
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
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(now).UtcDateTime,
                UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(now).UtcDateTime,
            };
        }

        // TODO: Переделать в bulk
        public IReadOnlyList<TrackRecord> CreateMany(IReadOnlyCollection<Track> files)
        {
            List<TrackRecord> result = [];

            foreach (Track file in files)
            {
                result.Add(Create(file));
            }

            return result;
        }

        public bool Delete(Guid id)
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "delete from Track where TrackId=$TrackId;";
            command.Parameters.AddWithValue("$TrackId", id.ToString());

            return command.ExecuteNonQuery() == 1;
        }

        public void DeleteAll()
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "delete * from Track;";
            command.ExecuteNonQuery();
        }

        public void DeleteMany(IReadOnlyCollection<Guid> ids)
        {
            using var command = _connection.CreateCommand();
            var parameters = ids.Select((id, i) => new { id, name = $"id{i}" }).ToList();
            string params_sql = $"{string.Join(", ", parameters.Select(x => x.name))}";

            command.CommandText = $"delete * from Tracks where TrackId in ({params_sql})";

            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.name, param.id.ToString());
            }

            command.ExecuteNonQuery();
        }

        public TrackRecord Get(Guid id)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "select * from Track where TrackId=$TrackId;";
            command.Parameters.AddWithValue("$TrackId", id.ToString());

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                throw new KeyNotFoundException($"<{this}> Record with id='{id}' does not exist");
            }

            return SerializeSchema(reader);
        }

        public IReadOnlyList<TrackRecord> GetAll()
        {
            List<TrackRecord> result = [];

            using var command = _connection.CreateCommand();
            command.CommandText = "select * from Track;";

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var model = SerializeSchema(reader);
                if (model != null)
                {
                    result.Add(model);
                }
            }

            return result;
        }

        public IReadOnlyList<TrackRecord> GetMany(IReadOnlyCollection<Guid> ids)
        {
            using var command = _connection.CreateCommand();

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
                var model = SerializeSchema(reader);

                if (model != null)
                {
                    result.Add(model);
                }
            }

            return result;
        }

        private static TrackRecord SerializeSchema(DbDataReader reader)
        {
            return new TrackRecord
            {
                TrackId = Guid.Parse(reader["TrackId"].ToString()!),

                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(int.Parse(reader["CreatedAt"].ToString()!)).DateTime,

                UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(int.Parse(reader["UpdatedAt"].ToString()!)).DateTime,

                Title = (string)reader["Title"],

                Artist = (string)reader["Artist"],

                FilePath = (string)reader["FilePath"],

                Cover = reader["Cover"] is DBNull ? null : (byte[])reader["Cover"],

                Album = reader["Album"].ToString(),

                AlbumArtist = reader["AlbumArtist"].ToString(),

                Duration = TimeSpan.FromSeconds(int.Parse(reader["Duration"].ToString()!)),

                Genres = JsonSerializer.Deserialize<string[]>(reader["Genres"].ToString()!)!,

                Year = reader["Year"] is DBNull ? null : int.Parse(reader["Year"].ToString()!),

                TrackNumber = reader["TrackNumber"] is DBNull ? null : int.Parse(reader["TrackNumber"].ToString()!),

                DiscNumber = reader["DiscNumber"] is DBNull ? null : int.Parse(reader["DiscNumber"].ToString()!),
            };
        }
    }
}
