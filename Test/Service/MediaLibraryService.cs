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

        public PlaylistRecord CreatePlaylist(Playlist playlist)
        {
            //...
        }

        public PlaylistRecord GetPlaylist(string id)
        {
            //...
        }

        public IReadOnlyList<PlaylistRecord> GetAllPlaylists()
        {
            //...
        }

        public TrackRecord ImportTrack(Track track)
        {
            //...
        }

        public IReadOnlyList<TrackRecord> ImportTracks(List<Track> tracks)
        {
            //...
        }

        public bool DeleteTrack()
        {
            //...
        }

        public void DeleteTracks() 
        { 
            //...
        }

        public void DeleteAllTracks()
        {
            //...
        }

    }
}
