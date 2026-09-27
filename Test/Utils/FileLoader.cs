using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
using Test.Schema;
using Windows.Storage.Streams;
using System;


namespace Test.Utils
{
    public class FileLoader
    {
        private byte[] defaultCoverArt = System.IO.File.ReadAllBytes(@"C:/Users/evges/source/repos/Test/Test/Assets/CoverArt/default.png");

        public Track LoadFromFile(string filePath)
        {
            var file = File.Create(filePath);
            var fileName = System.IO.Path.GetFileName(filePath);

            string title = file.Tag.Title;
            string artist = file.Tag.FirstPerformer;

            // Парсить из тегов
            if (file.Tag.Title == null)
            {
                title = fileName.Replace(".mp3", "");

                title = fileName.Split("-")[0];
                artist = fileName.Split("-")[1];

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
        }

        private byte[] GetFrontCover(File file)
        {
            if (file.Tag.Pictures.Length > 0)
            {
                var picture = file.Tag.Pictures[0];
                return picture.Data.Data;
            }
            return defaultCoverArt;
        }

        public List<Track> LoadFromFolder(string folder)
        {
            string[] files = System.IO.Directory.GetFiles(folder) ?? [];
            List<Track> result = [];

            foreach (var file in files)
            {
                if (file.Contains(".mp3"))
                {
                    result.Add(LoadFromFile(file));
                }
            }

            return result;
        }

        public async Task<BitmapImage> BytesToBitmapImageAsync(byte[] bytes)
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
