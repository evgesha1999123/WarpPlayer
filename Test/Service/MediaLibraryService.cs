using System;
using System.Collections.Generic;
using Test.Repo;
using Test.Schema;
using Test.Utils;

namespace Test.Service
{
    public class MediaLibraryService
    {
        private readonly FileLoader _fileLoader;
        private readonly ITrackRepository<Track, TrackRecord> _trackRepository;
        private readonly DatabaseHealthChecker _databaseHealthChecker;

        public MediaLibraryService(FileLoader fileLoader, ITrackRepository<Track, TrackRecord> trackRepository, DatabaseHealthChecker dbHealthChecker)
        {
            _fileLoader = fileLoader;
            _trackRepository = trackRepository;
            _databaseHealthChecker = dbHealthChecker;
        }

        //public PlaylistRecord CreatePlaylist(Playlist playlist)
        //{
        //    //...
        //}

        //public PlaylistRecord GetPlaylist(string id)
        //{
        //    //...
        //}

        //public IReadOnlyList<PlaylistRecord> GetAllPlaylists()
        //{
        //    //...
        //}

        public TrackRecord ImportTrack(string filePath)
        {
            var track = _fileLoader.GetTrackData(filePath);
            return _trackRepository.Create(track);

        }

        public IReadOnlyList<TrackRecord> ImportTracks(List<string> filePaths)
        {
            List<Track> tracks = [];

            foreach (var path in filePaths)
            {
                var track = _fileLoader.GetTrackData(path);
                tracks.Add(track);
            }

            return _trackRepository.CreateMany(tracks);
        }

        public bool DeleteTrack(string id)
        {
            return _trackRepository.Delete(Guid.Parse(id));
        }

        public void DeleteTracks(List<string> ids) 
        {
            List<Guid> guids = [];

            foreach (var id in ids)
            {
                guids.Add(Guid.Parse(id));
            }

            _trackRepository.DeleteMany(guids);
        }

        public void DeleteAllTracks()
        {
            _trackRepository.DeleteAll();
        }

    }
}
