using System;


namespace Test.Schema
{
    public class Playlist 
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
    public class PlaylistRecord: Playlist, ITimestampable
    {
        public required Guid PlaylistId { get; init; }
        public Guid[] TrackIds { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
