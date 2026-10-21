using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Player.Database;
using Player.DependencyInjection;
using Player.Repo;
using Player.Schema;
using Player.Utils.Helper;
using System.Diagnostics;


namespace Player.Tests;

public sealed class PlaylistRepositoryTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly string _databasePath;
    private readonly ISqliteConnectionFactory _factory;
    private readonly DatabaseHealthChecker _healthChecker;
    private readonly PlaylistRepository _repository;

    public PlaylistRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"playlist-tests-{Guid.NewGuid():N}.db");

        _serviceProvider = TestServices.Build(_databasePath);

        _factory = _serviceProvider.GetRequiredService<ISqliteConnectionFactory>();
        _healthChecker = _serviceProvider.GetRequiredService<DatabaseHealthChecker>();

        _healthChecker.EnsurePlaylistTableCreated();
        _repository = new PlaylistRepository(_factory);
    }

    private static Playlist MakePlaylist(
        string name = "My playlist",
        string? description = "Test description")
    {
        return new Playlist
        {
            Name = name,
            Description = description
        };
    }


    [Fact]
    public void Create_ShouldSavePlaylistAndReturnRecord()
    {
        var playlist = MakePlaylist("Rock", "Rock music");

        var created = _repository.Create(playlist);

        Assert.NotEqual(Guid.Empty, created.PlaylistId);
        Assert.Equal("Rock", created.Name);
        Assert.Equal("Rock music", created.Description);
        Assert.NotEqual(default, created.CreatedAt);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);

        var loaded = _repository.Get(created.PlaylistId);

        Assert.Equal(created.PlaylistId, loaded.PlaylistId);
        Assert.Equal("Rock", loaded.Name);
        Assert.Equal("Rock music", loaded.Description);
    }

    [Fact]
    public void Create_ShouldSupportNullDescription()
    {
        var created = _repository.Create(
            MakePlaylist("No description", null));

        var loaded = _repository.Get(created.PlaylistId);

        Assert.Null(loaded.Description);
    }

    [Fact]
    public void Get_ShouldThrowWhenPlaylistDoesNotExist()
    {
        var id = Guid.NewGuid();

        Assert.Throws<KeyNotFoundException>(
            () => _repository.Get(id));
    }

    [Fact]
    public void GetAll_ShouldReturnAllPlaylists()
    {
        _repository.Create(MakePlaylist("Rock"));
        _repository.Create(MakePlaylist("Jazz"));
        _repository.Create(MakePlaylist("Pop"));

        var result = _repository.GetAll();

        Assert.Equal(3, result.Count);
        Assert.Contains(result, x => x.Name == "Rock");
        Assert.Contains(result, x => x.Name == "Jazz");
        Assert.Contains(result, x => x.Name == "Pop");
    }

    [Fact]
    public void GetAll_ShouldReturnEmptyListWhenDatabaseIsEmpty()
    {
        var result = _repository.GetAll();

        Assert.Empty(result);
    }


    [Fact]
    public void CreateMany_ShouldCreateAllPlaylists()
    {
        var playlists = new List<Playlist>
    {
        MakePlaylist("Rock"),
        MakePlaylist("Jazz"),
        MakePlaylist("Pop")
    };

        var created = _repository.CreateMany(playlists);

        Assert.Equal(3, created.Count);
        Assert.Equal(3, _repository.GetAll().Count);
        Assert.Equal(3, created.Select(x => x.PlaylistId).Distinct().Count());
    }

    [Fact]
    public void GetMany_ShouldReturnOnlyRequestedPlaylists()
    {
        var first = _repository.Create(MakePlaylist("Rock"));
        var second = _repository.Create(MakePlaylist("Jazz"));
        _repository.Create(MakePlaylist("Pop"));

        var result = _repository.GetMany(
            new[] { first.PlaylistId, second.PlaylistId });

        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.PlaylistId == first.PlaylistId);
        Assert.Contains(result, x => x.PlaylistId == second.PlaylistId);
    }

    [Fact]
    public void GetMany_ShouldReturnEmptyListForEmptyIds()
    {
        var result = _repository.GetMany(Array.Empty<Guid>());

        Assert.Empty(result);
    }

    [Fact]
    public void Delete_ShouldRemoveExistingPlaylist()
    {
        var created = _repository.Create(MakePlaylist());

        var deleted = _repository.Delete(created.PlaylistId);

        Assert.True(deleted);
        Assert.Empty(_repository.GetAll());
    }

    [Fact]
    public void Delete_ShouldReturnFalseWhenPlaylistDoesNotExist()
    {
        var deleted = _repository.Delete(Guid.NewGuid());

        Assert.False(deleted);
    }

    [Fact]
    public void DeleteMany_ShouldRemoveOnlyRequestedPlaylists()
    {
        var first = _repository.Create(MakePlaylist("Rock"));
        var second = _repository.Create(MakePlaylist("Jazz"));
        var third = _repository.Create(MakePlaylist("Pop"));

        _repository.DeleteMany(
            new[] { first.PlaylistId, second.PlaylistId });

        var remaining = _repository.GetAll();

        foreach (var r in remaining)
        {
            Debug.WriteLine(r.Name);
        }

        var onlyRemaining = Assert.Single(remaining);
        Assert.Equal(third.PlaylistId, onlyRemaining.PlaylistId);
    }

    [Fact]
    public void DeleteAll_ShouldRemoveEveryPlaylist()
    {
        _repository.Create(MakePlaylist("Rock"));
        _repository.Create(MakePlaylist("Jazz"));

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
