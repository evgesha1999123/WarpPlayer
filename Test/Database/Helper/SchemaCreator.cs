using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.IO;


namespace Test.Database.Helper
{
    public class SchemaCreator
    {
       private readonly SqliteConnection _connection;

        public SchemaCreator(SqliteConnection connection)
        {
            _connection = connection;
        }

        public void GenerateSchemas() 
        {
            string[] files = Directory.GetFiles("Query/CreateIfNotExists/");

            foreach (string file in files)
            {
                Debug.WriteLine(file);
            }
        }
    }
}