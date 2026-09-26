using System.Collections.Generic;
using Test.Schema;

public class PlaybackQueue
{
    private readonly List<TrackRecord> _queue = [];
    private int _currentIndex = -1;

    public int Size => _queue.Count;

    public TrackRecord? GetCurrent()
    {
        if (_currentIndex < 0 || _currentIndex >= Size)
            return null;

        return _queue[_currentIndex];
    }

    public void Add(TrackRecord track)
    {
        _queue.Add(track);

        // Если очередь была пустой,
        // первый добавленный становится current.
        if (_currentIndex == -1)
            _currentIndex = 0;
    }

    public bool Delete(TrackRecord track)
    {
        var index = _queue.IndexOf(track);

        if (index < 0)
            return false;

        bool deletingCurrent = index == _currentIndex;

        _queue.RemoveAt(index);

        if (Size == 0)
        {
            _currentIndex = -1;
            return true;
        }

        if (index < _currentIndex)
        {
            // Удалили элемент перед current.
            // Current физически сдвинулся на -1.
            _currentIndex--;
        }
        else if (deletingCurrent)
        {
            // Current удалён.
            // Если есть следующий — индекс остаётся тем же.
            // Если удалили последний — переходим на предыдущий.
            if (_currentIndex >= Size)
                _currentIndex = Size - 1;
        }

        return true;
    }

    public TrackRecord? Move(int delta)
    {
        if (Size == 0)
            return null;

        var newIndex = _currentIndex + delta;

        if (newIndex < 0 || newIndex >= Size)
            return _queue[_currentIndex];

        _currentIndex = newIndex;

        return _queue[_currentIndex];
    }

    public bool SetCurrent(string filePath)
    {
        var index = _queue.FindIndex(x => x.FilePath == filePath);

        if (index < 0)
            return false;

        _currentIndex = index;
        return true;
    }

    public IReadOnlyList<TrackRecord> DisplayQueue()
    {
        return _queue;
    }
}