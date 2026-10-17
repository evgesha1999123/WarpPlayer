using System;
using System.Collections.Generic;
using System.Diagnostics;
using Player.Schema;


namespace Player.Utils
{
    public enum PlaybackMode
    {
        Normal,
        RepeatAll,
        Shuffle
    }

    public class PlaybackQueue
    {
        private readonly List<TrackRecord> _tracks = [];
        private readonly List<TrackRecord> _previousTracks = [];
        private const int MAX_PREVIOUS_QUEUE_CAPACITY = 255;
        private int _currentIndex = -1;

        public PlaybackMode PlaybackMode { get; private set; } = PlaybackMode.Normal;

        public int Size => _tracks.Count;
        public bool IsEmpty => _tracks.Count == 0;

        public bool IsCurrentValid => IsValidIndex(_currentIndex);

        public bool IsFirst => IsCurrentValid && _currentIndex == 0;
        public bool IsLast => IsCurrentValid && _currentIndex == _tracks.Count - 1;

        public bool HasPrevious => _previousTracks.Count > 0;

        public TrackRecord? Current => IsCurrentValid ? _tracks[_currentIndex] : null;
        public void TogglePlaybackMode()
        {
            PlaybackMode = PlaybackMode switch
            {
                PlaybackMode.Normal => PlaybackMode.Shuffle,
                PlaybackMode.Shuffle => PlaybackMode.RepeatAll,
                PlaybackMode.RepeatAll => PlaybackMode.Normal,
                _ => PlaybackMode.Normal
            };
        }

        public void Add(TrackRecord track)
        {
            ArgumentNullException.ThrowIfNull(track);

            _tracks.Add(track);

            if (!IsCurrentValid)
            {
                _currentIndex = 0;
            }
        }

        public bool Delete(TrackRecord track)
        {
            ArgumentNullException.ThrowIfNull(track);

            var index = _tracks.IndexOf(track);

            if (index < 0)
                return false;

            var deletingCurrent = index == _currentIndex;

            _tracks.RemoveAt(index);
            _previousTracks.Remove(track);

            if (IsEmpty)
            {
                InvalidateCurrent();
                return true;
            }

            if (deletingCurrent)
            {
                InvalidateCurrent();
            }
            else if (index < _currentIndex)
            {
                _currentIndex--;
            }

            return true;
        }

        public void InvalidateCurrent()
        {
            _currentIndex = -1;
        }

        public TrackRecord? GetNext()
        {
            return Move(1);
        }

        public TrackRecord? GetPrevious()
        {
            return Move(-1);
        }

        private TrackRecord? Move(int direction)
        {
            if (IsEmpty)
                return null;

            if (PlaybackMode == PlaybackMode.Shuffle)
                return MoveShuffleMode(direction);

            if (!IsCurrentValid)
                return SetCurrentByDirection(direction);

            var newIndex = _currentIndex + direction;

            if (PlaybackMode == PlaybackMode.RepeatAll)
                newIndex = (newIndex + Size) % Size;

            else if (!IsValidIndex(newIndex))
                return null;

            _currentIndex = newIndex;
            return Current;
        }

        private TrackRecord? MoveShuffleMode(int direction)
        {
            if (direction < 0)
                return MoveToPreviousShuffleTrack();

            if (IsCurrentValid)
                PushPrevious(Current!);

            return MoveShuffle();
        }

        private TrackRecord? MoveToPreviousShuffleTrack()
        {
            var previous = PopPrevious();

            if (previous is null)
                return null;

            _currentIndex = _tracks.IndexOf(previous);

            return IsCurrentValid ? Current : null;
        }

        private TrackRecord SetCurrentByDirection(int direction)
        {
            _currentIndex = direction > 0 ? 0 : Size - 1;
            return Current!;
        }

        private TrackRecord MoveShuffle()
        {
            if (Size == 1)
            {
                _currentIndex = 0;
                return _tracks[0];
            }

            int newIndex;

            do
            {
                newIndex = Random.Shared.Next(Size);
            }
            while (newIndex == _currentIndex);

            _currentIndex = newIndex;

            return _tracks[_currentIndex];
        }

        public bool SetCurrent(string filePath)
        {
            var index = _tracks.FindIndex(
                track => string.Equals(
                    track.FilePath,
                    filePath,
                    StringComparison.OrdinalIgnoreCase));

            if (index < 0)
                return false;

            if (index == _currentIndex)
                return true;

            if (PlaybackMode == PlaybackMode.Shuffle && IsCurrentValid)
            {
                PushPrevious(_tracks[_currentIndex]);
            }

            _currentIndex = index;
            return true;
        }

        public IReadOnlyList<TrackRecord> GetTracks() => _tracks;

        private void PushPrevious(TrackRecord track)
        {
            if (_previousTracks.Count >= MAX_PREVIOUS_QUEUE_CAPACITY)
            {
                _previousTracks.RemoveAt(0);
            }
            _previousTracks.Add(track);
        }

        private TrackRecord? PopPrevious()
        {
            if (_previousTracks.Count > 0)
            {
                TrackRecord track = _previousTracks[^1];
                _previousTracks.RemoveAt(_previousTracks.Count - 1);
                return track;
            }
            return null;
        }

        public TrackRecord? GetTrackAfterCurrentIsRemoved()
        {
            if (!IsCurrentValid)
                return null;

            if (_currentIndex < _tracks.Count - 1)
            {
                return _tracks[_currentIndex + 1];
            }

            if (_currentIndex > 0)
            {
                return _tracks[_currentIndex - 1];
            }

            return null;
        }

        public void Clear()
        {
            _tracks.Clear();
            _previousTracks.Clear();
            InvalidateCurrent();
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _tracks.Count;
        }
    }
}