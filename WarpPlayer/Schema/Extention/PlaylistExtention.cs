using Player.Schema;
using System;
using System.Data.Common;

public static class DbDataReaderExtensions
{
    public static PlaylistRecord ToPlaylistRecord(this DbDataReader reader)
    {
        return new PlaylistRecord
        {
            PlaylistId = Guid.Parse(reader["PlaylistId"].ToString()!),
            Name = reader["Name"].ToString()!,
            Description = reader["Description"] is DBNull ? null : reader["Description"].ToString(),
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(reader["CreatedAt"].ToString()!)).UtcDateTime,
            UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(reader["UpdatedAt"].ToString()!)).UtcDateTime,
        };
    }
}