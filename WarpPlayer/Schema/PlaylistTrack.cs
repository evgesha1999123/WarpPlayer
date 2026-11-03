using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Player.Schema
{
    public record class PlaylistTrack
    {
        public required Guid TrackId;
        public required Guid PlaylistId;
    }
}
