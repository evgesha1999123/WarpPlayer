using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using static Player.Settings.ResourceManifest;


namespace Player.Settings
{
    public class ApplicationSettings
    {
        private readonly string configFilePath = Path.Combine(AppContext.BaseDirectory, "ResourceManifest.xml");
        private XDocument Document { get;  init; }
        public ApplicationSettings()
        {
            Document = XDocument.Load(configFilePath);
        }

        public DatabaseResources GetDatabase()
        {
            var source = Document?
                .Root?
                .Element("Resources")?
                .Element("Schema")?
                .Attribute("Source");

            var schemas = Document?
                .Root?
                .Element("Resources")?
                .Element("Schema")?
                .Elements("Table")
                .Attributes("Name");

            var database = new Dictionary<string, string>();

            database[source!.Name.ToString()] = Path.Combine(AppContext.BaseDirectory, $"{source.Value}.db");

            foreach (var s in schemas!)
            {
                database[s.Value.ToString()] = Path.Combine(AppContext.BaseDirectory, $"Database/Helper/Query/CreateIfNotExists/{s.Value}.sql");
            }

            return new DatabaseResources(database);
        }

        public ImageResources GetImages()
        {
            var result = Document?
                .Root?
                .Element("Resources")?
                .Element("Assets")?
                .Element("Images")?
                .Element("Image");

            var images = new Dictionary<string, string>
            {
                [result!.Attribute("Name")!.Value!.ToString()] = Path.Combine(AppContext.BaseDirectory, result!.Attribute("Path")!.Value!.ToString()!)
            };

            return new ImageResources(images);
        }
    }
}
