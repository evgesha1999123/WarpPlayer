

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Player.Settings
{
    public record class ResourceManifest
    {
        public record class DatabaseResources
        {
            public required Dictionary<string, string> Settings;


            [SetsRequiredMembers]
            public DatabaseResources(Dictionary<string, string> database)
            {
                Settings = database;
            }
        }

        public record class ImageResources
        {
            public required Dictionary<string, string> Settings;


            [SetsRequiredMembers]
            public ImageResources(Dictionary<string, string> images)
            {
                Settings = images;
            }
        }

        public required DatabaseResources Database { get; init; }
        public required ImageResources Images { get; init; }

        public ResourceManifest(DatabaseResources database, ImageResources images)
        {
            Database = database;
            Images = images;
        }
    }
}
