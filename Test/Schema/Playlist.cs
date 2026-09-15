using System;


namespace Test.Schema
{
    public record class Playlist: ITimestampable
    {
        string? PlaylistId;
        string[]? TrackIds;
        string? Name;
        string? Description;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
