using Player.Schema;
using Player.Utils;


namespace Player.Tests;

public sealed class PlaybackQueueTests
{
    private static TrackRecord MakeTrack(
        string filePath,
        string title = "Test Track")
    {
        return new TrackRecord
        {
            TrackId = Guid.NewGuid(),
            Title = title,
            Artist = "Test Artist",
            FilePath = filePath,
            Duration = TimeSpan.FromSeconds(180),
            Genres = Array.Empty<string>(),
            Cover = Array.Empty<byte>(),
        };
    }

    [Fact]
    public void NewQueue_ShouldBeEmpty()
    {
        var queue = new PlaybackQueue();

        Assert.Equal(0, queue.Size);
        Assert.True(queue.IsEmpty);
        Assert.False(queue.IsCurrentValid);
        Assert.Null(queue.Current);
        Assert.False(queue.IsLast);
    }

    [Fact]
    public void Add_ShouldAddTrackAndMakeItCurrent()
    {
        var queue = new PlaybackQueue();
        var track = MakeTrack("track1.mp3");

        queue.Add(track);

        Assert.Equal(1, queue.Size);
        Assert.False(queue.IsEmpty);
        Assert.True(queue.IsCurrentValid);
        Assert.Same(track, queue.Current);
        Assert.True(queue.IsLast);
    }

    [Fact]
    public void Add_ShouldNotChangeCurrentTrack_WhenQueueAlreadyHasCurrent()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);

        Assert.Equal(2, queue.Size);
        Assert.Same(first, queue.Current);
    }

    [Fact]
    public void Add_ShouldThrow_WhenTrackIsNull()
    {
        var queue = new PlaybackQueue();

        Assert.Throws<ArgumentNullException>(
            () => queue.Add(null!));
    }

    [Fact]
    public void GetNext_ShouldMoveToNextTrack()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");
        var third = MakeTrack("track3.mp3");

        queue.Add(first);
        queue.Add(second);
        queue.Add(third);

        Assert.Same(second, queue.GetNext());
        Assert.Same(third, queue.GetNext());
    }

    [Fact]
    public void GetNext_ShouldReturnNull_WhenAlreadyAtLastTrack()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);

        queue.GetNext();

        Assert.Same(second, queue.Current);
        Assert.Null(queue.GetNext());
        Assert.Same(second, queue.Current);
    }

    [Fact]
    public void GetPrevious_ShouldMoveToPreviousTrack()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");
        var third = MakeTrack("track3.mp3");

        queue.Add(first);
        queue.Add(second);
        queue.Add(third);

        queue.GetNext();
        queue.GetNext();

        Assert.Same(second, queue.GetPrevious());
        Assert.Same(first, queue.GetPrevious());
    }

    [Fact]
    public void GetPrevious_ShouldReturnNull_WhenAlreadyAtFirstTrack()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);

        Assert.Same(first, queue.Current);
        Assert.Null(queue.GetPrevious());
        Assert.Same(first, queue.Current);
    }

    [Fact]
    public void GetNext_ShouldReturnNull_WhenQueueIsEmpty()
    {
        var queue = new PlaybackQueue();

        Assert.Null(queue.GetNext());
    }

    [Fact]
    public void GetPrevious_ShouldReturnNull_WhenQueueIsEmpty()
    {
        var queue = new PlaybackQueue();

        Assert.Null(queue.GetPrevious());
    }

    [Fact]
    public void GetNext_WhenCurrentIsInvalid_ShouldSelectFirstTrack()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);
        queue.InvalidateCurrent();

        Assert.False(queue.IsCurrentValid);

        Assert.Same(first, queue.GetNext());
        Assert.Same(first, queue.Current);
    }

    [Fact]
    public void GetPrevious_WhenCurrentIsInvalid_ShouldSelectLastTrack()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);
        queue.InvalidateCurrent();

        Assert.Same(second, queue.GetPrevious());
        Assert.Same(second, queue.Current);
    }

    [Fact]
    public void SetCurrent_ShouldSelectTrackByFilePath()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);

        Assert.True(queue.SetCurrent("track2.mp3"));
        Assert.Same(second, queue.Current);
    }

    [Fact]
    public void SetCurrent_ShouldBeCaseInsensitive()
    {
        var queue = new PlaybackQueue();

        var track = MakeTrack(@"C:\Music\Track.mp3");
        queue.Add(track);

        Assert.True(queue.SetCurrent(@"c:\music\TRACK.MP3"));
        Assert.Same(track, queue.Current);
    }

    [Fact]
    public void SetCurrent_ShouldReturnFalse_WhenTrackDoesNotExist()
    {
        var queue = new PlaybackQueue();

        queue.Add(MakeTrack("track1.mp3"));

        Assert.False(queue.SetCurrent("missing.mp3"));
    }

    [Fact]
    public void SetCurrent_ShouldNotChangeCurrent_WhenSameTrackIsSelected()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        queue.Add(first);

        Assert.True(queue.SetCurrent("track1.mp3"));
        Assert.Same(first, queue.Current);
    }

    [Fact]
    public void Delete_ShouldRemoveTrack()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);

        Assert.True(queue.Delete(second));

        Assert.Equal(1, queue.Size);
        Assert.DoesNotContain(second, queue.GetTracks());
        Assert.Same(first, queue.Current);
    }

    [Fact]
    public void Delete_ShouldReturnFalse_WhenTrackDoesNotExist()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var missing = MakeTrack("missing.mp3");

        queue.Add(first);

        Assert.False(queue.Delete(missing));
        Assert.Equal(1, queue.Size);
        Assert.Same(first, queue.Current);
    }

    [Fact]
    public void Delete_ShouldInvalidateCurrent_WhenCurrentTrackIsDeleted()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);

        Assert.True(queue.Delete(first));

        Assert.Equal(1, queue.Size);
        Assert.False(queue.IsCurrentValid);
        Assert.Null(queue.Current);
    }

    [Fact]
    public void Delete_ShouldMoveCurrentIndex_WhenTrackBeforeCurrentIsDeleted()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");
        var third = MakeTrack("track3.mp3");

        queue.Add(first);
        queue.Add(second);
        queue.Add(third);

        queue.GetNext();
        Assert.Same(second, queue.Current);

        queue.Delete(first);

        Assert.Same(second, queue.Current);
        Assert.Equal(2, queue.Size);
    }

    [Fact]
    public void Delete_ShouldInvalidateCurrent_WhenLastRemainingTrackIsDeleted()
    {
        var queue = new PlaybackQueue();

        var track = MakeTrack("track1.mp3");
        queue.Add(track);

        Assert.True(queue.Delete(track));

        Assert.True(queue.IsEmpty);
        Assert.False(queue.IsCurrentValid);
        Assert.Null(queue.Current);
    }

    [Fact]
    public void Delete_ShouldThrow_WhenTrackIsNull()
    {
        var queue = new PlaybackQueue();

        Assert.Throws<ArgumentNullException>(
            () => queue.Delete(null!));
    }

    [Fact]
    public void GetTracks_ShouldReturnAllTracks()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");
        var third = MakeTrack("track3.mp3");

        queue.Add(first);
        queue.Add(second);
        queue.Add(third);

        var tracks = queue.GetTracks();

        Assert.Equal(3, tracks.Count);
        Assert.Same(first, tracks[0]);
        Assert.Same(second, tracks[1]);
        Assert.Same(third, tracks[2]);
    }

    [Fact]
    public void Clear_ShouldRemoveAllTracksAndInvalidateCurrent()
    {
        var queue = new PlaybackQueue();

        queue.Add(MakeTrack("track1.mp3"));
        queue.Add(MakeTrack("track2.mp3"));

        queue.Clear();

        Assert.True(queue.IsEmpty);
        Assert.Equal(0, queue.Size);
        Assert.False(queue.IsCurrentValid);
        Assert.Null(queue.Current);
        Assert.Empty(queue.GetTracks());
    }

    [Fact]
    public void InvalidateCurrent_ShouldMakeCurrentInvalid()
    {
        var queue = new PlaybackQueue();

        var track = MakeTrack("track1.mp3");
        queue.Add(track);

        queue.InvalidateCurrent();

        Assert.False(queue.IsCurrentValid);
        Assert.Null(queue.Current);
    }

    [Fact]
    public void IsLast_ShouldBeTrue_WhenCurrentIsLastTrack()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);

        Assert.False(queue.IsLast);

        queue.GetNext();

        Assert.True(queue.IsLast);
    }

    [Fact]
    public void IsLast_ShouldBeFalse_WhenCurrentIsInvalid()
    {
        var queue = new PlaybackQueue();

        queue.Add(MakeTrack("track1.mp3"));
        queue.InvalidateCurrent();

        Assert.False(queue.IsLast);
    }

    [Fact]
    public void GetTrackAfterCurrentIsRemoved_ShouldReturnNextTrack()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");
        var third = MakeTrack("track3.mp3");

        queue.Add(first);
        queue.Add(second);
        queue.Add(third);

        queue.GetNext();

        Assert.Same(third, queue.GetTrackAfterCurrentIsRemoved());
    }

    [Fact]
    public void GetTrackAfterCurrentIsRemoved_ShouldReturnPreviousTrack_WhenCurrentIsLast()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);
        queue.GetNext();

        Assert.Same(first, queue.GetTrackAfterCurrentIsRemoved());
    }

    [Fact]
    public void GetTrackAfterCurrentIsRemoved_ShouldReturnNull_WhenOnlyTrackExists()
    {
        var queue = new PlaybackQueue();

        queue.Add(MakeTrack("track1.mp3"));

        Assert.Null(queue.GetTrackAfterCurrentIsRemoved());
    }

    [Fact]
    public void GetTrackAfterCurrentIsRemoved_ShouldReturnNull_WhenCurrentIsInvalid()
    {
        var queue = new PlaybackQueue();

        queue.Add(MakeTrack("track1.mp3"));
        queue.Add(MakeTrack("track2.mp3"));
        queue.InvalidateCurrent();

        Assert.Null(queue.GetTrackAfterCurrentIsRemoved());
    }

    [Fact]
    public void TogglePlaybackMode_ShouldCycleThroughAllModes()
    {
        var queue = new PlaybackQueue();

        Assert.Equal(PlaybackMode.Normal, queue.PlaybackMode);

        queue.TogglePlaybackMode();
        Assert.Equal(PlaybackMode.Shuffle, queue.PlaybackMode);

        queue.TogglePlaybackMode();
        Assert.Equal(PlaybackMode.RepeatOne, queue.PlaybackMode);

        queue.TogglePlaybackMode();
        Assert.Equal(PlaybackMode.RepeatAll, queue.PlaybackMode);

        queue.TogglePlaybackMode();
        Assert.Equal(PlaybackMode.Normal, queue.PlaybackMode);
    }

    [Fact]
    public void RepeatAll_ShouldWrapFromLastToFirst()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);

        queue.TogglePlaybackMode();
        queue.TogglePlaybackMode();
        queue.TogglePlaybackMode();

        Assert.Equal(PlaybackMode.RepeatAll, queue.PlaybackMode);

        queue.GetNext();

        Assert.Same(second, queue.Current);

        queue.GetNext();

        Assert.Same(first, queue.Current);
    }

    [Fact]
    public void RepeatAll_ShouldWrapFromFirstToLast()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);

        queue.TogglePlaybackMode();
        queue.TogglePlaybackMode();
        queue.TogglePlaybackMode();

        Assert.Same(first, queue.Current);

        Assert.Same(second, queue.GetPrevious());
        Assert.Same(first, queue.GetNext());
    }

    [Fact]
    public void Shuffle_ShouldMoveToAnotherTrack()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");

        queue.Add(first);
        queue.Add(second);

        queue.TogglePlaybackMode();

        var current = queue.Current;
        var next = queue.GetNext();

        Assert.NotNull(next);
        Assert.NotSame(current, next);
    }

    [Fact]
    public void Shuffle_WithSingleTrack_ShouldKeepSameTrack()
    {
        var queue = new PlaybackQueue();

        var track = MakeTrack("track1.mp3");
        queue.Add(track);

        queue.TogglePlaybackMode();

        Assert.Same(track, queue.GetNext());
        Assert.Same(track, queue.Current);
    }

    [Fact]
    public void Shuffle_GetPrevious_ShouldReturnPreviousTrack()
    {
        var queue = new PlaybackQueue();

        var first = MakeTrack("track1.mp3");
        var second = MakeTrack("track2.mp3");
        var third = MakeTrack("track3.mp3");

        queue.Add(first);
        queue.Add(second);
        queue.Add(third);

        queue.TogglePlaybackMode();

        var firstShuffle = queue.GetNext();
        var secondShuffle = queue.GetNext();

        Assert.NotNull(firstShuffle);
        Assert.NotNull(secondShuffle);
        Assert.NotSame(firstShuffle, secondShuffle);

        Assert.Same(firstShuffle, queue.GetPrevious());
    }
}