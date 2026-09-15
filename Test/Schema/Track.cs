using System;
using System.Drawing;

namespace Test.Schema
{
    public record class Track: ITimestampable
    {
        string? TrackId;
        string? Title;
        Image? Icon;
        string? Artist;
        string? Album;
        string? AlbumArtist;
        string? FilePath;
        string[]? Genres;
        int? Year;
        int? TrackNumber;
        int? DiscNumber;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
