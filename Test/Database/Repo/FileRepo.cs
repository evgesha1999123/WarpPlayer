using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
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
            List<TrackRecord> tracks = [];

            foreach (Track file in files)
            {
                tracks.Add(Create(file));
            }

            return tracks;
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

        // TODO: Переделать в bulk
        public void DeleteMany(IReadOnlyCollection<Guid> ids)
        {
            foreach (var id in ids)
            {
                Delete(id);
            }
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
            // ...
        }

        public IReadOnlyList<TrackRecord> GetAll()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<TrackRecord> GetMany(IReadOnlyCollection<Guid> guids)
        {
            throw new NotImplementedException();
        }
    }
}
