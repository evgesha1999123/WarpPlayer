using System;


namespace Test.Schema
{
    public interface ITimestampable
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}
