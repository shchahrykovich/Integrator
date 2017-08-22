using System;
using System.Data.SqlClient;
using System.Data.SqlClient.Tests;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            using (TestTdsServer server = TestTdsServer.StartTestServer())
            {
                using (SqlConnection connection = new SqlConnection(server.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = "SELECT [name], [state] FROM [sys].[databases] WHERE [name] = db_name();";

                        connection.Open();
                        var output = cmd.ExecuteScalar();
                    }
                }
            }
        }
    }
}
