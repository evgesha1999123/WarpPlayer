using System;
using System.Collections.Generic;
using Test.Schema;


namespace Test.Utils
{
    public class PlaybackQueue
    {
        private readonly List<TrackRecord> _queue = [];
        private int _currentIndex = 0;
        public int Size => _queue.Count;

        public readonly int NextTrackIndexDelta = +1;
        public readonly int PrevTrackIndexDelta = -1;

        private bool IsLast { get => _currentIndex == Size - 1; }
        private bool IsFirst { get => _currentIndex == 0; }
        private bool HasNext { get => !IsLast; }
        private bool HasPrevious { get => !IsFirst; }
        private bool HasTracks { get => _queue.Count > 0; }

        public void Add(TrackRecord track)
        {
            _queue.Add(track);
        }

        public void Delete(TrackRecord track)
        {
            _queue.Remove(track);
        }

        public TrackRecord? Move(int delta)
        {
            return HasTracks ? MoveAndGetTrack(delta) : null;
        }

        private TrackRecord MoveAndGetTrack(int delta)
        {
            bool canMove = (delta < 0) ? HasPrevious : HasNext;
            _currentIndex = canMove ? _currentIndex + delta : _currentIndex;
            return _queue[_currentIndex];
        }

        public IReadOnlyList<TrackRecord> DisplayQueue()
        {
            return _queue;
        }
    }
}
