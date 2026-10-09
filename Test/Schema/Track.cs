using System;


namespace Test.Schema
{
    public class Track 
    {
        public required string Title { get; set; }
        public required string Artist { get; set; }
        public required string FilePath { get; set; }
        public required TimeSpan Duration { get; set; }
        public required byte[] Cover { get; init; }
        public string? Album { get; set; }
        public string? AlbumArtist { get; set; }
        public string[] Genres { get; set; } = [];
        public int? Year { get; set; }
        public int? TrackNumber { get; set; }
        public int? DiscNumber { get; set; }
    }
    public class TrackRecord: Track, ITimestampable
    {
        public required Guid TrackId { get; init; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
