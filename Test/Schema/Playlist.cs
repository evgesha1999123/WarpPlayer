using System;


namespace Test.Schema
{
    public class Playlist: ITimestampable
    {
        public string? PlaylistId { get; set; }
        public string[]? TrackIds { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
