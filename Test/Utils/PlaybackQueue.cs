using System.Collections.Generic;
using Test.Schema;


namespace Test.Utils
{
    public class PlaybackQueue
    {
        private readonly List<TrackRecord> _queue = [];
        private int _currentIndex = 0;
        public int Size = 0;

        public PlaybackQueue()
        {

        }

        public void Add(TrackRecord track)
        {
            _queue.Add(track);
            Size++;
        }

        public void Delete(TrackRecord track)
        {
            _queue.Remove(track);
            Size--;
        }

        public TrackRecord? GetNext()
        {
            if (HasTracks())
            {
                var track = _queue[_currentIndex];

                if (CanGetNext())
                {
                    _currentIndex++;
                }

                return track;
            }

            return null;
        }

        public TrackRecord? GetPrevious()
        {
            if (HasTracks())
            {
                var track = _queue[_currentIndex];

                if (CanGetPrevious())
                {
                    _currentIndex--;
                }

                return track;
            }

            return null;
        }

        public List<TrackRecord> DisplayQueue()
        {
            return _queue;
        }

        private bool IsLast()
        {
            return _currentIndex == Size;
        }

        private bool IsFirst()
        {
            return _currentIndex == 0;
        }

        private bool CanGetNext()
        {
            return (!IsLast());
        }

        private bool CanGetPrevious()
        {
            return(!IsFirst());
        }

        private bool HasTracks()
        {
            return Size != 0;
        }
    }
}
