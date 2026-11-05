using System;


namespace Player.Schema
{
    public record class PlaylistTrack
    {
        public required Guid TrackId;
        public required Guid PlaylistId;
    }
}