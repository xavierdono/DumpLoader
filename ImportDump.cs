using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DumpLoader
{
    public class ImportDump
    {
        public ImportDump(ServerOptions options, string path)
        {
            var logicalName = GetLogicalNameFromDump(options.CONNECTION_STRING, path);

            using (SqlConnection connection = new SqlConnection(options.CONNECTION_STRING))
            {
                string database = Path.GetFileNameWithoutExtension(path);
                string sql = @$"IF DB_ID('{database}') IS NOT NULL DROP DATABASE [{database}];
                                RESTORE DATABASE [{database}] FROM DISK = '{path}'
                                WITH MOVE '{logicalName.Key}' TO '{Path.Combine(options.LOCATION_DATA, database)}.mdf',
                                MOVE '{logicalName.Value}' TO '{Path.Combine(options.LOCATION_LOG, database)}.ldf';";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public KeyValuePair<string, string> GetLogicalNameFromDump(string connectionString, string path)
        {
            var logicalName = new Dictionary<string, string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sql = $"RESTORE FILELISTONLY FROM DISK = '{path}'";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    logicalName.Add(reader.GetString(reader.GetOrdinal("Type")), reader.GetString(reader.GetOrdinal("LogicalName")));
                }
                
                reader.Close();
            }

            return new KeyValuePair<string, string>(logicalName["D"], logicalName["L"]);
        }
    }
}