using System.Collections.Generic;

namespace Player.Schema
{
    public record class PlaylistWithTracks
    {
        public required PlaylistRecord Playlist;
        public required IReadOnlyList<TrackRecord> Tracks;
    }
}
