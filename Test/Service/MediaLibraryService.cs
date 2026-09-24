using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            _databaseHealthChecker.CheckTablesExists();
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

        public IReadOnlyList<TrackRecord> LookupLibrary()
        {
            return _trackRepository.GetAll();
        }

        public TrackRecord ImportTrack(string filePath)
        {
            var track = _fileLoader.LoadFromFile(filePath);
            return _trackRepository.Create(track);

        }

        public IReadOnlyList<TrackRecord> ImportTracks(string folder)
        {
            var tracks = _fileLoader.LoadFromFolder(folder);
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
