using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test.Repo;
using Test.Schema;
using Test.Utils.File;
using Test.Utils.Helper;


namespace Test.Service
{
    public class MediaLibraryService
    {
        public record class ImportResponse
        {
            public IReadOnlyList<TrackRecord> TrackRecords { get; init; }
            public int Skipped { get; init; }

            public ImportResponse(IReadOnlyList<TrackRecord> tracks, int skipped)
            {
                TrackRecords = tracks;
                Skipped = skipped;
            }
        }

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

        public async Task<BitmapImage> GetCoverArt(byte[] imgBytes)
        {
            return await _fileLoader.BytesToBitmapImageAsync(imgBytes);
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

        public ImportResponse ImportTracks(IReadOnlyList<string> files)
        {
            var existingPaths = new List<string>(LookupLibrary().Select(f => f.FilePath));
            HashSet<string> existing = new HashSet<string>(existingPaths);
            HashSet<string> newFiles = new HashSet<string>(files);
            newFiles.ExceptWith(existing);

            var tracks = new List<Track>();
            foreach (var file in newFiles)
            {
                var track = _fileLoader.LoadFromFile(file);
                tracks.Add(track);
            }

            var trackRecords = _trackRepository.CreateMany(tracks);

            var candidates = files.Count;
            var diff = candidates - newFiles.Count;

            return new ImportResponse(trackRecords, diff);
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
