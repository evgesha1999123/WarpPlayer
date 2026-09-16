using System;
using System.Drawing;

namespace Test.Schema
{
    public class Track: ITimestampable
    {
        public string? TrackId { get; set; }
        public string? Title { get; set; }
        public Image? Icon { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public string? AlbumArtist { get; set; }
        public TimeSpan Duration { get; set; }
        public string? FilePath { get; set; }
        public string[]? Genres { get; set; }
        public int? Year { get; set; }
        public int? TrackNumber { get; set; }
        public int? DiscNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
