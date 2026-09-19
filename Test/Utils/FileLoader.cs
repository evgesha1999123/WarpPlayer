using TagLib;
using Test.Schema;


namespace Test.Utils
{
    public class FileLoader
    {
        public Track GetTrackData(string filePath)
        {
            var file = File.Create(filePath);

            return new Track
            {
                Title = file.Tag.Title,
                Artist = file.Tag.FirstPerformer,
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

        private static byte[]? GetFrontCover(File file)
        {
            if (file.Tag.Pictures.Length > 0)
            {
                var picture = file.Tag.Pictures[0];
                return picture.Data.Data;
            }
            return null;
        }
    }
}
