using Microsoft.Data.Sqlite;
using System.Diagnostics;


namespace Test.Database
{
    public class TestConnection
    {
        public static void Ping(string connectionString = "Data Source= playlist.db")
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();
            Debug.WriteLine($"!!!!!!!!!! CONNECTION: {connection.DataSource}");
            connection.Close();
        }
    }
}
