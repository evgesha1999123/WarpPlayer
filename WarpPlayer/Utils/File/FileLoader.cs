using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.VisualBasic.Logging;
using Player.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using static Player.Settings.ResourceManifest;


namespace Player.Utils.File
{
    public class FileLoader
    {
        private readonly byte[] defaultCoverArt;

        public record class FileLoaderResponse
        {
            public IReadOnlyList<Track> Success = [];
            public IReadOnlyDictionary<string, string> Errors = new Dictionary<string, string>();   // filePath, Detail

            public FileLoaderResponse(IReadOnlyList<Track> success, IReadOnlyDictionary<string, string> errors)
            {
                Success = success;
                Errors = errors;
            }
        }

        public class Union<Track, Error>
        {
            public Track? Value1 { get; }
            public Error? Value2 { get; }

            public bool IsTrack { get; }
            public bool IsError => !IsTrack;

            public Union(Track value)
            {
                Value1 = value;
                IsTrack = true;
            }

            public Union(Error value)
            {
                Value2 = value;
                IsTrack = false;
            }

            public static implicit operator Union<Track, Error>(Track value) => new(value);
            public static implicit operator Union<Track, Error>(Error value) => new(value);
        }

        public FileLoader(ImageResources images)
        {
            defaultCoverArt = System.IO.File.ReadAllBytes(images.Settings["DefaultCoverArt"]);
        }

        public Union<Track, string> LoadFromFile(string filePath)
        {
            try
            {
                var file = TagLib.File.Create(filePath);
                var fileName = System.IO.Path.GetFileName(filePath);

                string title = file.Tag.Title;
                string artist = file.Tag.FirstPerformer;

                if (file.Tag.Title == null)
                {
                    title = fileName.Replace(".mp3", "");

                    if (!title.Contains('-'))
                    {
                        artist = "Неизвестный исполнитель";
                    }
                    else
                    {
                        title = fileName.Split("-")[0];
                        artist = fileName.Split("-")[1];
                    }
                }

                return new Track
                {
                    Title = title,
                    Artist = artist,
                    FilePath = filePath,
                    Duration = file.Properties.Duration,
                    Cover = GetFrontCover(file),
                    Album = file.Tag.Album,
                    AlbumArtist = file.Tag.FirstAlbumArtist,
                    Genres = file.Tag.Genres,
                    Year = (int)(file.Tag.Year),
                    TrackNumber = (int)file.Tag.Track,
                    DiscNumber = (int)file.Tag.Disc,
                };
            } catch (TagLib.CorruptFileException err) {
                Debug.WriteLine(err.StackTrace);
                Debug.WriteLine(err.Message);
                if (err.InnerException != null)
                {
                    Debug.WriteLine(err.StackTrace);
                    Debug.WriteLine($"Inner exception: {err.InnerException.Message}");
                }
                return err.Message;
            }
        }

        private byte[] GetFrontCover(TagLib.File file)
        {
            if (file.Tag.Pictures.Length > 0)
            {
                var picture = file.Tag.Pictures[0];
                return picture.Data.Data;
            }
            return defaultCoverArt;
        }

        public FileLoaderResponse BulkLoadFromFiles(HashSet<string> files)
        {
            return BulkLoadFromFiles(files.ToList<string>());
        }

        public FileLoaderResponse BulkLoadFromFiles(IReadOnlyList<string> files)
        {
            List<Track> success = [];
            Dictionary<string, string> errors = new Dictionary<string, string>();

            foreach (var file in files)
            {
                if (file != null)
                {
                    var track = LoadFromFile(file);
                    if (track.IsTrack)
                    {
                        success.Add(track.Value1!);
                    } else {
                        errors[file] = track.Value2!;
                    }
                }
            }

            return new FileLoaderResponse(success, errors);
        }

        public async Task<BitmapImage> GetImageAsync(byte[] bytes)
        {
            using var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(bytes.AsBuffer());

            stream.Seek(0);

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);

            return bitmap;
        }
    }
}
