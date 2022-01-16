using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DumpLoader
{
    public class ServerOptions
    {
        public readonly string CONNECTION_STRING;
        public readonly string LOCATION_DATA;
        public readonly string LOCATION_LOG;

        public ServerOptions()
        {
            CONNECTION_STRING = GetConnectionString();

            var location = GetLocationServer();       
            LOCATION_DATA = location.Key;
            LOCATION_LOG = location.Value;

            LogConsole.LogInformation("Get info server...", "OK");
        }

        public string GetConnectionString()
        {
            var fileName = "DumpLoader.json";
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            var jsonString = File.ReadAllText(path);
            ConnectionString connectionString = JsonSerializer.Deserialize<ConnectionString>(jsonString);

            return $"Server={connectionString.Server};Database={connectionString.Database};User Id={connectionString.User};Password={connectionString.Password};";
        }

        public KeyValuePair<string, string> GetLocationServer()
        {
            var locationData = string.Empty;
            var locationLog = string.Empty;

            using (SqlConnection connection = new SqlConnection(this.CONNECTION_STRING))
            {
                string sql = "SELECT SERVERPROPERTY('InstanceDefaultDataPath'), SERVERPROPERTY('InstanceDefaultLogPath')";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    locationData = reader.GetString(0);
                    locationLog = reader.GetString(1);
                }
                
                reader.Close();
            }

            return new KeyValuePair<string, string>(locationData, locationLog);
        }
    }
}