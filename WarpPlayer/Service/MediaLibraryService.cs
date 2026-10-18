using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Player.Repo;
using Player.Schema;
using Player.Utils.File;
using Player.Utils.Helper;
using static Player.Utils.File.FileLoader;
using Microsoft.UI.Xaml.Controls;


namespace Player.Service
{
    public class MediaLibraryService
    {
        public record class ImportResponse
        {
            public IReadOnlyList<TrackRecord> Success { get; init; }
            public IReadOnlyList<Skipped> Skipped = [];

            public ImportResponse(IReadOnlyList<TrackRecord> success, IReadOnlyList<Skipped> skipped)
            {
                Success = success;
                Skipped = skipped;
            }

            public int DuplicatesCount => Skipped.Where(s => s.Reason == SkipReason.Duplicate).ToList().Count;
            public int ErrorCount => Skipped.Where(s => s.Reason == SkipReason.Error).ToList().Count;
            public IReadOnlyList<FileLoadError> Errors => Skipped.Where(s => s.Reason == SkipReason.Error).Select(s => new FileLoadError(s.FilePath, s.Detail)).ToList();

            
            public record class FileLoadError
            {
                public string FilePath;
                public string Detail;

                public FileLoadError(string filePath, string detail)
                {
                    FilePath = filePath;
                    Detail = detail;
                }
            }
        }

        public record class Skipped
        {
            public string FilePath;
            public SkipReason Reason;
            public string Detail;

            public Skipped(string filePath, SkipReason reason, string detail)
            {
                FilePath = filePath;
                Reason = reason;
                Detail = detail;
            }
        }

        public enum SkipReason
        {
            Duplicate,
            Error
        }

        private readonly FileLoader _fileLoader;
        private readonly ITrackRepository<Track, TrackRecord> _trackRepository;
        private readonly IPlaylistRepository<Playlist, PlaylistRecord> _playlistRepository;
        private readonly DatabaseHealthChecker _databaseHealthChecker;

        public MediaLibraryService(
            FileLoader fileLoader, 
            ITrackRepository<Track, TrackRecord> 
            trackRepository, 
            DatabaseHealthChecker dbHealthChecker,
            IPlaylistRepository<Playlist, PlaylistRecord> playlistRepository
        )
        {
            _fileLoader = fileLoader;
            _trackRepository = trackRepository;
            _playlistRepository = playlistRepository;
            _databaseHealthChecker = dbHealthChecker;

            _databaseHealthChecker.CheckTablesExists();
        }

        public async Task<BitmapImage> GetCoverArt(byte[] imgBytes)
        {
            return await _fileLoader.GetImageAsync(imgBytes);
        }

        public PlaylistRecord CreatePlaylist(Playlist playlist)
        {
            return _playlistRepository.Create(playlist);
        }

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
            return _trackRepository.GetAll().Where(f => FileValidator.IsExist(f.FilePath)).ToList();
        }

        public ImportResponse ImportTracks(IReadOnlyList<string> files)
        {
            var existingPaths = new List<string>(LookupLibrary().Select(f => f.FilePath));
            var newFiles = GetUnique(existingPaths, files);

            FileLoaderResponse loaderResponse = _fileLoader.BulkLoadFromFiles(newFiles);
            var trackRecords = _trackRepository.CreateMany(loaderResponse.Success);

            return new ImportResponse(trackRecords, GetSkipped(existingPaths, loaderResponse));
        }


        private HashSet<string> GetUnique(List<string>existingPaths, IReadOnlyList<string> files)
        {
            HashSet<string> existing = new HashSet<string>(existingPaths);
            HashSet<string> newFiles = new HashSet<string>(files);
            newFiles.ExceptWith(existing);
            return newFiles;
        }
        private IReadOnlyList<Skipped> GetSkipped(List<string> existingPaths, FileLoaderResponse loaderResponse)
        {
            List<Skipped> existing_ = [];
            List<Skipped> errors = [];

            foreach (var file in existingPaths)
            {
                existing_.Add(new Skipped(file, SkipReason.Duplicate, "This file already exists"));
            }

            foreach ((var filePath, var errDetail) in loaderResponse.Errors)
            {
                errors.Add(new Skipped(filePath, SkipReason.Error, errDetail));
            }
            
            errors.AddRange(existing_);
            return errors;
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
