using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Player.Database;
using Player.DependencyInjection;
using Player.Repo;
using Player.Schema;
using Player.Utils.Helper;


namespace WarpPlayerTest
{
    public sealed class TestPlaylistTrackRepository : IDisposable
    {
        private ServiceProvider _serviceProvider;
        private readonly string _databasePath;
        private readonly ISqliteConnectionFactory _factory;
        private readonly DatabaseHealthChecker _healthChecker;
        private readonly PlaylistTrackRepository _playlistTrackRepository;
        private readonly TrackRepository _trackRepository;
        private readonly PlaylistRepository _playlistRepository;

        public TestPlaylistTrackRepository()
        {
            _databasePath = Path.Combine(Path.GetTempPath(), $"test-database-{DateTime.Now.Second.ToString()}.db");
            _serviceProvider = TestServices.Build(_databasePath);

            _factory = _serviceProvider.GetRequiredService<ISqliteConnectionFactory>();
            _healthChecker = _serviceProvider.GetRequiredService<DatabaseHealthChecker>();

            _healthChecker.CheckAllTablesExists();

            _playlistTrackRepository = new PlaylistTrackRepository(_factory);
            _trackRepository = new TrackRepository(_factory);
            _playlistRepository = new PlaylistRepository(_factory);
        }

        [Fact]
        public void AddOneTrackToPlaylist_ShouldSaveAndReturnRecord()
        {
            Guid trackId = CreateAndGetRandomTrack().TrackId;
            Guid playlistId = CreateAndGetRandomPlaylist().PlaylistId;

            var record = _playlistTrackRepository.AddOneTrackToPlaylist(playlistId, trackId);

            Assert.Equal(trackId, record.TrackId);
            Assert.Equal(playlistId, record.PlaylistId);
            Assert.IsType<PlaylistTrack>(record);
        }

        [Fact]
        public void AddManyTracksToPlaylist_ShouldSaveAndReturnRecords()
        {
            const int SIZE = 255;

            List<Track> tracks = [];

            for (int i = 0; i < SIZE; i++)
            {
                tracks.Add(GenerateTrack());
            }

            Playlist playlist = GeneratePlaylist();

            IReadOnlyList<TrackRecord> trackRecords = _trackRepository.CreateMany(tracks);
            PlaylistRecord playlistRecord = _playlistRepository.Create(playlist);

            Guid playlistId = playlistRecord.PlaylistId;
            List<Guid> trackIds = trackRecords.Select(t => t.TrackId).ToList();

            var result = _playlistTrackRepository.AddManyTracksToPlaylist(playlistId, trackIds);

            Assert.NotNull(result);
            Assert.Equal(SIZE, result.Count);

            Assert.All(result, record =>
            {
                Assert.Equal(playlistId, record.PlaylistId);
                Assert.Contains(record.TrackId, trackIds);
            });

            Assert.Equal(SIZE, result.Select(r => r.TrackId).Distinct().Count());
        }

        [Fact]
        public void GetPlaylistById_ShouldReturnRecord()
        {
            // Arrange
            TrackRecord trackRecord = CreateAndGetRandomTrack();
            PlaylistRecord playlistRecord = CreateAndGetRandomPlaylist();

            _playlistTrackRepository.AddOneTrackToPlaylist(
                playlistRecord.PlaylistId,
                trackRecord.TrackId
            );

            // Act
            IReadOnlyList<PlaylistTrack> playlistTracks =
                _playlistTrackRepository.GetPlaylistById(playlistRecord.PlaylistId);

            // Assert
            Assert.NotNull(playlistTracks);
            Assert.Single(playlistTracks);

            PlaylistTrack record = playlistTracks.First();

            Assert.Equal(playlistRecord.PlaylistId, record.PlaylistId);
            Assert.Equal(trackRecord.TrackId, record.TrackId);
        }

        [Fact]
        public void GetAllPlaylists_ShouldGetAllPlaylistGuidsWithAllTrackGuids()
        {
            // Arrange
            const int PLAYLIST_COUNT = 3;
            const int TRACK_COUNT = 5;

            List<PlaylistRecord> playlists = [];
            List<TrackRecord> tracks = [];

            for (int i = 0; i < PLAYLIST_COUNT; i++)
            {
                playlists.Add(CreateAndGetRandomPlaylist());
            }

            for (int i = 0; i < TRACK_COUNT; i++)
            {
                tracks.Add(CreateAndGetRandomTrack());
            }

            // Создаём связи:
            // Playlist 0 -> Tracks 0, 1
            // Playlist 1 -> Tracks 2, 3
            // Playlist 2 -> Track 4

            _playlistTrackRepository.AddManyTracksToPlaylist(
                playlists[0].PlaylistId,
                [tracks[0].TrackId, tracks[1].TrackId]
            );

            _playlistTrackRepository.AddManyTracksToPlaylist(
                playlists[1].PlaylistId,
                [tracks[2].TrackId, tracks[3].TrackId]
            );

            _playlistTrackRepository.AddOneTrackToPlaylist(
                playlists[2].PlaylistId,
                tracks[4].TrackId
            );

            // Act
            IReadOnlyList<PlaylistTrack> result =
                _playlistTrackRepository.GetAllPlaylists();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(PLAYLIST_COUNT, result.Select(p => p.PlaylistId).Distinct().Count());

            Assert.Equal(
                PLAYLIST_COUNT,
                result.Select(p => p.PlaylistId).Distinct().Count()
            );

            Assert.Equal(
                TRACK_COUNT,
                result.Count
            );

            Assert.Equal(
                new[]
                {
            (playlists[0].PlaylistId, tracks[0].TrackId),
            (playlists[0].PlaylistId, tracks[1].TrackId),
            (playlists[1].PlaylistId, tracks[2].TrackId),
            (playlists[1].PlaylistId, tracks[3].TrackId),
            (playlists[2].PlaylistId, tracks[4].TrackId)
                }.OrderBy(x => x.Item1).ThenBy(x => x.Item2),
                result
                    .Select(p => (p.PlaylistId, p.TrackId))
                    .OrderBy(x => x.PlaylistId)
                    .ThenBy(x => x.TrackId)
            );
        }

        [Fact]
        public void DeletePlaylistById_ShouldDeletePlaylistAndBindedTracksFromPlaylist()
        {
            // Arrange
            PlaylistRecord playlistRecord = CreateAndGetRandomPlaylist();

            List<TrackRecord> tracks = [CreateAndGetRandomTrack(), CreateAndGetRandomTrack(), CreateAndGetRandomTrack()];

            _playlistTrackRepository.AddManyTracksToPlaylist(
                playlistRecord.PlaylistId,
                tracks.Select(t => t.TrackId).ToList()
            );

            // Act
            _playlistTrackRepository.DeletePlaylistById(playlistRecord.PlaylistId);

            // Assert
            IReadOnlyList<PlaylistTrack> playlistTracks =
                _playlistTrackRepository.GetPlaylistById(playlistRecord.PlaylistId);

            Assert.Empty(playlistTracks);
        }

        private TrackRecord CreateAndGetRandomTrack() 
        {
            return _trackRepository.Create(GenerateTrack());
        }

        private static Track GenerateTrack()
        {
            return new Track
            {
                Title = Guid.NewGuid().ToString(),
                Album = Guid.NewGuid().ToString(),
                Cover = GetRandomImage(),
                Artist = Guid.NewGuid().ToString(),
                FilePath = Guid.NewGuid().ToString(),
                Duration = TimeSpan.FromSeconds(Random.Shared.Next(1, (int)Math.Pow(2, 16)))
            };
        }

        private PlaylistRecord CreateAndGetRandomPlaylist()
        {
            return _playlistRepository.Create(GeneratePlaylist());
        }

        private static Playlist GeneratePlaylist()
        {
            return new Playlist
            {
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
            };
        }

        private static byte[] GetRandomImage()
        {
            int size = Random.Shared.Next(1, 20_000);
            byte[] img = new byte[size];

            Random.Shared.NextBytes(img);

            return img;
        }


        public void Dispose()
        {
            SqliteConnection.ClearAllPools();

            if (File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }
        }
    }
}
