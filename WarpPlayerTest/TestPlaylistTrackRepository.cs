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
        public void AddOne_ShouldSaveAndReturnRecord()
        {
            Guid trackId = CreateAndGetRandomTrack().TrackId;
            Guid playlistId = CreateAndGetRandomPlaylist().PlaylistId;

            var record = _playlistTrackRepository.AddOne(playlistId, trackId);

            Assert.Equal(trackId, record.TrackId);
            Assert.Equal(playlistId, record.PlaylistId);
            Assert.IsType<PlaylistTrack>(record);
        }

        private TrackRecord CreateAndGetRandomTrack() 
        {
            return _trackRepository.Create(
                new Track
                {
                    Title = Guid.NewGuid().ToString(),
                    Album = Guid.NewGuid().ToString(),
                    Cover = GetRandomImage(),
                    Artist = Guid.NewGuid().ToString(),
                    FilePath = Guid.NewGuid().ToString(),
                    Duration = TimeSpan.FromSeconds(Random.Shared.Next(1, (int)Math.Pow(2, 16)))
                }
            );
        }

        private PlaylistRecord CreateAndGetRandomPlaylist()
        {
            return _playlistRepository.Create(
                new Playlist
                {
                    Name = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                }
            );
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
