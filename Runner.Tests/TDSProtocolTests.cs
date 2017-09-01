using Microsoft.SqlServer.TDS.Servers;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using Runner.TDS;
using Xunit;

namespace Runner.Tests
{
    public class TDSProtocolTests
    {
        [Fact]
        public void Test1()
        {
            var arguments = new TDSServerArguments { Log = Console.Out };
            var engine = new StaticQueryEngine(arguments);
            int timeout = Debugger.IsAttached ? 15 : 15;
            using (var server = TestTdsServer.StartTestServer(engine, 5463, timeout))
            {
                using (SqlConnection connection = new SqlConnection(server.ConnectionString))
                {
                    connection.Open();

                    SqlTransaction transaction = connection.BeginTransaction("Transaction1");

                    SqlCommand command = connection.CreateCommand();
                    command.Transaction = transaction;

                    command.CommandText = "insert into Table (Name) VALUES ('Bob')";
                    command.ExecuteNonQuery();

                    command.CommandText = "insert into Table (Name) VALUES ('Joe')";
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
        }
    }
}
