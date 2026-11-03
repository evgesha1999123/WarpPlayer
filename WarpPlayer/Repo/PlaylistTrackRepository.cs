using Player.Database;
using Player.Schema;
using System;


namespace Player.Repo
{
    public class PlaylistTrackRepository
    {
        ISqliteConnectionFactory connectionFactory;
        public PlaylistTrackRepository(ISqliteConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        public PlaylistTrack AddOne(Guid playlistId, Guid trackId)
        {
            using var connection = connectionFactory.Create();
            connection.Open();
            var command = connection.CreateCommand();

            command.CommandText = "insert into PlaylistTrack(PlaylistId, TrackId) values ($PlaylistId, $TrackId);";
            command.Parameters.AddWithValue($"PlaylistId", playlistId.ToString());
            command.Parameters.AddWithValue($"TrackId", trackId.ToString());

            int insertedRows = command.ExecuteNonQuery();

            if (insertedRows == 1)
            {
                return new PlaylistTrack { PlaylistId = playlistId, TrackId = trackId };
            }
            throw new InvalidOperationException($"Запись PlaylistTrack с PlaylistId={playlistId}, TrackId={trackId} не была создана.");
        }
    }
}
