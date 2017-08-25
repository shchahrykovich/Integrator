using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace TestApp
{
    class Program
    {
        static int Main(string[] args)
        {
            var cmd = String.Empty;
            if(1 <= args.Length)
            {
                cmd = args[0];
            }

            if(2 <= args.Length)
            {
                Debugger.Launch();
            }

            try
            {
                return RunCommand(cmd);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 10000;
            }
        }

        private static int RunCommand(String cmd)
        {
            var connectionString =
                "Data Source=localhost,56434;Initial Catalog=UserProfiles;Timeout=60000;Encrypt=False;";

            Console.WriteLine("Executing: " + cmd);

            switch (cmd)
            {
                case "execute-transaction":
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
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
                    break;
                }
                case "execute-reader-with-parameters":
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = conn;
                            command.CommandText = "dbo.GetAllUsers";
                            command.CommandType = CommandType.StoredProcedure;

                            var p = new SqlParameter("name", SqlDbType.NVarChar, 255);
                            p.Value = "test-test";
                            command.Parameters.Add(p);

                            p = new SqlParameter("name2", SqlDbType.NVarChar, 255);
                            p.Value = "test-test2";
                            command.Parameters.Add(p);

                            conn.Open();

                            using (SqlDataReader rdr = command.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    if (rdr.GetInt32(0) != 1)
                                    {
                                        return 1;
                                    }

                                    if (rdr.GetString(1) != "Bob")
                                    {
                                        return 2;
                                    }
                                }
                            }
                        }
                        break;
                    }

                case "execute-scalar":
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.GetAllUsers";
                        command.CommandType = CommandType.StoredProcedure;

                        conn.Open();
                        var reader = command.ExecuteScalar();
                        var number = Int32.Parse(reader.ToString());

                        Console.WriteLine("Result is - " + number);

                        if (number != 101)
                        {
                            return 5;
                        }
                    }
                    break;
                }
                case "execute-non-query":
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "dbo.GetAllUsers";
                        command.CommandType = CommandType.StoredProcedure;

                        conn.Open();
                        var reader = command.ExecuteNonQuery();
                        var number = Int32.Parse(reader.ToString());

                        Console.WriteLine("Result is - " + number);

                        if (number != 101)
                        {
                            return 5;
                        }
                    }
                    break;
                }
                case "user-profile":
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = conn;
                        command.CommandText = "select * from user-profiles";

                        conn.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            if (reader.GetInt32(0) != 1)
                            {
                                return 1;
                            }

                            if (reader.GetString(1) != "Bob")
                            {
                                return 2;
                            }
                        }
                    }
                    break;
                }
                default:
                    return 111111;
            }

            return 0;
        }
    }
}
