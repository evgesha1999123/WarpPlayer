using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Player.Database;
using Player.DependencyInjection;
using Player.Repo;
using Player.Schema;
using Player.Utils.Helper;
using WarpPlayerTest;

namespace Player.Tests;


public sealed class TrackRepositoryTests : IDisposable
{
    private ServiceProvider _serviceProvider;
    private readonly string _databasePath;
    private readonly ISqliteConnectionFactory _factory;
    private readonly DatabaseHealthChecker _healthChecker;
    private readonly TrackRepository _repository;

    public TrackRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"track-tests-{Guid.NewGuid():N}.db");
        _serviceProvider = TestServices.Build(_databasePath);

        _factory = _serviceProvider.GetRequiredService<ISqliteConnectionFactory>();
        _healthChecker = _serviceProvider.GetRequiredService<DatabaseHealthChecker>();

        _healthChecker.EnsureTrackTableCreated();

        _repository = new TrackRepository(_factory);
    }

    private static Track MakeTrack(
        string title = "Test Track",
        string artist = "Test Artist")
    {
        return new Track
        {
            Title = title,
            Cover = new byte[] { 1, 2, 3, 4 },
            Artist = artist,
            Album = "Test Album",
            AlbumArtist = "Test Album Artist",
            Duration = TimeSpan.FromSeconds(180),
            FilePath = $@"C:\Music\{title}.mp3",
            Genres = new[] { "Rock", "Alternative" },
            Year = 2024,
            TrackNumber = 1,
            DiscNumber = 1
        };
    }

    [Fact]
    public void Create_ShouldSaveTrackAndReturnRecord()
    {
        var track = MakeTrack("Rock Song", "Rock Band");

        var created = _repository.Create(track);

        Assert.NotEqual(Guid.Empty, created.TrackId);
        Assert.Equal("Rock Song", created.Title);
        Assert.Equal("Rock Band", created.Artist);
        Assert.Equal("Test Album", created.Album);
        Assert.Equal("Test Album Artist", created.AlbumArtist);
        Assert.Equal(TimeSpan.FromSeconds(180), created.Duration);
        Assert.Equal(track.FilePath, created.FilePath);
        Assert.Equal(track.Cover, created.Cover);
        Assert.Equal(track.Genres, created.Genres);
        Assert.Equal(track.Year, created.Year);
        Assert.Equal(track.TrackNumber, created.TrackNumber);
        Assert.Equal(track.DiscNumber, created.DiscNumber);
        Assert.NotEqual(default, created.CreatedAt);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);

        var loaded = _repository.Get(created.TrackId);

        Assert.Equal(created.TrackId, loaded.TrackId);
        Assert.Equal(created.Title, loaded.Title);
        Assert.Equal(created.Artist, loaded.Artist);
        Assert.Equal(created.Album, loaded.Album);
        Assert.Equal(created.AlbumArtist, loaded.AlbumArtist);
        Assert.Equal(created.Duration, loaded.Duration);
        Assert.Equal(created.FilePath, loaded.FilePath);
        Assert.Equal(created.Cover, loaded.Cover);
        Assert.Equal(created.Genres, loaded.Genres);
        Assert.Equal(created.Year, loaded.Year);
        Assert.Equal(created.TrackNumber, loaded.TrackNumber);
        Assert.Equal(created.DiscNumber, loaded.DiscNumber);
    }

    [Fact]
    public void Create_ShouldSupportNullOptionalFields()
    {
        var track = MakeTrack();

        track.Album = null;
        track.AlbumArtist = null;
        track.Year = null;
        track.TrackNumber = null;
        track.DiscNumber = null;

        var created = _repository.Create(track);
        var loaded = _repository.Get(created.TrackId);

        Assert.Null(loaded.Album);
        Assert.Null(loaded.AlbumArtist);
        Assert.Null(loaded.Year);
        Assert.Null(loaded.TrackNumber);
        Assert.Null(loaded.DiscNumber);
    }

    [Fact]
    public void Get_ShouldThrowWhenTrackDoesNotExist()
    {
        Assert.Throws<KeyNotFoundException>(
            () => _repository.Get(Guid.NewGuid()));
    }

    [Fact]
    public void GetAll_ShouldReturnAllTracks()
    {
        _repository.Create(MakeTrack("Rock"));
        _repository.Create(MakeTrack("Jazz"));
        _repository.Create(MakeTrack("Pop"));

        var result = _repository.GetAll();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x.Title == "Rock");
        Assert.Contains(result, x => x.Title == "Jazz");
        Assert.Contains(result, x => x.Title == "Pop");
    }

    [Fact]
    public void GetAll_ShouldReturnEmptyListWhenDatabaseIsEmpty()
    {
        Assert.Empty(_repository.GetAll());
    }

    [Fact]
    public void CreateMany_ShouldCreateAllTracks()
    {
        var tracks = new List<Track>
        {
            MakeTrack("Rock"),
            MakeTrack("Jazz"),
            MakeTrack("Pop")
        };

        var created = _repository.CreateMany(tracks);

        Assert.Equal(3, created.Count);
        Assert.Equal(3, _repository.GetAll().Count);

        Assert.Equal(
            3,
            created.Select(x => x.TrackId).Distinct().Count());
    }

    [Fact]
    public void GetMany_ShouldReturnOnlyRequestedTracks()
    {
        var first = _repository.Create(MakeTrack("Rock"));
        var second = _repository.Create(MakeTrack("Jazz"));
        _repository.Create(MakeTrack("Pop"));

        var result = _repository.GetMany(
            new[] { first.TrackId, second.TrackId });

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.TrackId == first.TrackId);
        Assert.Contains(result, x => x.TrackId == second.TrackId);
    }

    [Fact]
    public void GetMany_ShouldReturnEmptyListForEmptyIds()
    {
        var result = _repository.GetMany(Array.Empty<Guid>());

        Assert.Empty(result);
    }

    [Fact]
    public void Delete_ShouldRemoveExistingTrack()
    {
        var created = _repository.Create(MakeTrack());

        var deleted = _repository.Delete(created.TrackId);

        Assert.True(deleted);
        Assert.Empty(_repository.GetAll());
    }

    [Fact]
    public void Delete_ShouldReturnFalseWhenTrackDoesNotExist()
    {
        Assert.False(_repository.Delete(Guid.NewGuid()));
    }

    [Fact]
    public void DeleteMany_ShouldRemoveOnlyRequestedTracks()
    {
        var first = _repository.Create(MakeTrack("Rock"));
        var second = _repository.Create(MakeTrack("Jazz"));
        var third = _repository.Create(MakeTrack("Pop"));

        _repository.DeleteMany(
            new[] { first.TrackId, second.TrackId });

        var remaining = _repository.GetAll();

        var onlyRemaining = Assert.Single(remaining);
        Assert.Equal(third.TrackId, onlyRemaining.TrackId);
    }

    [Fact]
    public void DeleteAll_ShouldRemoveEveryTrack()
    {
        _repository.Create(MakeTrack("Rock"));
        _repository.Create(MakeTrack("Jazz"));

        _repository.DeleteAll();

        Assert.Empty(_repository.GetAll());
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
