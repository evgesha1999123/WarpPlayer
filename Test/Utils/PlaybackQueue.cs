using System;
using System.Collections.Generic;
using Test.Schema;


namespace Test.Utils
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
        public bool IsFirst => _currentIndex == 0;
        public bool IsLast => _currentIndex == _tracks.Count - 1;
        public bool HasNext => _currentIndex >= 0 && _currentIndex < _tracks.Count - 1;

        public TrackRecord? Current => IsValidIndex(_currentIndex) ? _tracks[_currentIndex] : null;
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

            // Первый добавленный трек становится текущим.
            if (_currentIndex == -1)
            {
                _currentIndex = 0;
            }
        }

        public bool Delete(TrackRecord track)
        {
            var index = _tracks.IndexOf(track);

            if (index < 0)
                return false;

            _tracks.RemoveAt(index);

            if (IsEmpty)
            {
                _currentIndex = -1;
                return true;
            }

            if (index < _currentIndex)
            {
                // Удалили трек перед текущим.
                // Индекс текущего сдвинулся на один назад.
                _currentIndex--;
            }
            else if (index == _currentIndex && _currentIndex >= _tracks.Count)
            {
                // Удалили последний текущий трек.
                // Теперь текущим становится предыдущий.
                _currentIndex = _tracks.Count - 1;
            }

            return true;
        }

        public TrackRecord? GetNext()
        {
            return Move(1);
        }

        public TrackRecord? GetPrevious()
        {
            return Move(-1);
        }

        private TrackRecord? Move(int offset)
        {
            if (Size <= 1)
            {
                return null;
            }

            if (PlaybackMode == PlaybackMode.Shuffle)
            {
                return GetShuffled();
            }

            var newIndex = _currentIndex + offset;

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

        private TrackRecord GetShuffled()
        {
            int newIndex;

            while (true)
            {
                newIndex = Random.Shared.Next(0, Size);
                if (_currentIndex != newIndex) break;
            }

            _currentIndex = newIndex;
            return _tracks[_currentIndex];
        }

        public bool SetCurrent(string filePath)
        {
            var index = _tracks.FindIndex(track => track.FilePath == filePath);

            if (index < 0)
                return false;

            _currentIndex = index;
            return true;
        }

        public IReadOnlyList<TrackRecord> GetTracks()
        {
            return _tracks;
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _tracks.Count;
        }

        public TrackRecord? GetTrackAfterCurrentIsRemoved()
        {
            if (Current == null)
                return null;

            // Если после current есть следующий,
            // он станет новым current.
            if (HasNext)
            {
                return _tracks[_currentIndex + 1];
            }

            // Если current последний,
            // новым current станет предыдущий.
            if (_currentIndex > 0)
            {
                return _tracks[_currentIndex - 1];
            }

            // Это был единственный трек.
            return null;
        }

        public void Clear()
        {
            _tracks.Clear();
            _currentIndex = -1;
        }
    }
}