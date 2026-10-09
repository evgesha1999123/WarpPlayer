using System;


namespace Player.Schema
{
    public interface ITimestampable
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}
