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
            var connestionString =
                "Data Source=localhost,56434;Initial Catalog=UserProfiles;Timeout=60000;Encrypt=False;";

            Console.WriteLine("Executing: " + cmd);

            switch (cmd)
            {
                case "execute-scalar":
                {
                    using (SqlConnection conn = new SqlConnection(connestionString))
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
                    using (SqlConnection conn = new SqlConnection(connestionString))
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
                    using (SqlConnection conn = new SqlConnection(connestionString))
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
