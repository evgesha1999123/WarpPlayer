using System;
using System.Collections.Generic;
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
        private int _currentIndex = -1;

        public PlaybackMode PlaybackMode { get; private set; } = PlaybackMode.Normal;

        public int Size => _tracks.Count;
        public bool IsEmpty => _tracks.Count == 0;

        public bool IsCurrentValid => IsValidIndex(_currentIndex);

        public bool IsFirst =>
            IsCurrentValid && _currentIndex == 0;

        public bool IsLast =>
            IsCurrentValid && _currentIndex == _tracks.Count - 1;

        public bool HasNext =>
            Size > 0 &&
            (
                !IsCurrentValid ||
                _currentIndex < _tracks.Count - 1 ||
                PlaybackMode == PlaybackMode.RepeatAll ||
                PlaybackMode == PlaybackMode.Shuffle
            );

        public bool HasPrevious =>
            Size > 0 &&
            (
                !IsCurrentValid ||
                _currentIndex > 0 ||
                PlaybackMode == PlaybackMode.RepeatAll ||
                PlaybackMode == PlaybackMode.Shuffle
            );

        public TrackRecord? Current => IsCurrentValid ? _tracks[_currentIndex] : null;

        public int CurrentIndex => _currentIndex;

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
            {
                return MoveShuffle();
            }

            // Sentinel state:
            // current отсутствует, но очередь не пустая.
            //
            // Next  -> первый трек.
            // Previous -> последний трек.
            if (!IsCurrentValid)
            {
                _currentIndex = direction > 0 ? 0 : _tracks.Count - 1;
                return _tracks[_currentIndex];
            }

            var newIndex = _currentIndex + direction;

            if (PlaybackMode == PlaybackMode.RepeatAll)
            {
                newIndex = (newIndex + Size) % Size;
            }
            else if (!IsValidIndex(newIndex))
            {
                return null;
            }

            _currentIndex = newIndex;

            return _tracks[_currentIndex];
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

            _currentIndex = index;
            return true;
        }

        public IReadOnlyList<TrackRecord> GetTracks() => _tracks;

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
            InvalidateCurrent();
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _tracks.Count;
        }
    }
}