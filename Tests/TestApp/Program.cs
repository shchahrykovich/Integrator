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

            switch (cmd)
            {
                case "user-profile-rpc":
                    {
                        using (SqlConnection conn = new SqlConnection("Data Source=localhost,56434;Initial Catalog=UserProfiles;Timeout=600;Encrypt=False;"))
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = conn;
                            command.CommandText = "dbo.GetAllUsers";
                            command.CommandType = CommandType.StoredProcedure;

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
                case "user-profile":
                    {
                        using (SqlConnection conn = new SqlConnection("Data Source=localhost,56434;Initial Catalog=UserProfiles;Timeout=600;Encrypt=False;"))
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = conn;
                            command.CommandText = "select * from user-profiles";

                            conn.Open();
                            SqlDataReader reader = command.ExecuteReader();
                            while (reader.Read())
                            {
                                if(reader.GetInt32(0) != 1)
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
                    break;
            }

            return 0;
        }
    }
}
