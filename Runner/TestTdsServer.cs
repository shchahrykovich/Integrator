// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using Microsoft.SqlServer.TDS.EndPoint;
using Microsoft.SqlServer.TDS.Servers;
using System.Net;
using System.Runtime.CompilerServices;

namespace System.Data.SqlClient.Tests
{
    public class TestTdsServer : GenericTDSServer, IDisposable
    {
        private TDSServerEndPoint _endpoint = null;

        private SqlConnectionStringBuilder connectionStringBuilder;

        public TestTdsServer(TDSServerArguments args) : base(args) { }

        public TestTdsServer(QueryEngine engine, TDSServerArguments args) : base(args)
        {
            this.Engine = engine;
        }

        public static TestTdsServer StartServerWithQueryEngine(QueryEngine engine, int port = 0, int timeout = 600, bool enableFedAuth = false, bool enableLog = false, [CallerMemberName] string methodName = "")
        {
            TDSServerArguments args = new TDSServerArguments()
            {
                Log = enableLog ? Console.Out : null,
            };

            if (enableFedAuth)
            {
                args.FedAuthRequiredPreLoginOption = Microsoft.SqlServer.TDS.PreLogin.TdsPreLoginFedAuthRequiredOption.FedAuthRequired;
            }

            TestTdsServer server = engine == null ? new TestTdsServer(args) : new TestTdsServer(engine, args);
            server._endpoint = new TDSServerEndPoint(server) { ServerEndPoint = new IPEndPoint(IPAddress.Any, port) };
            server._endpoint.EndpointName = methodName;
            // The server EventLog should be enabled as it logs the exceptions.
            server._endpoint.EventLog = Console.Out;
            server._endpoint.Start();

            server.connectionStringBuilder = new SqlConnectionStringBuilder() { DataSource = "localhost," + port, ConnectTimeout = timeout, Encrypt = false };
            server.ConnectionString = server.connectionStringBuilder.ConnectionString;
            return server;
        }

        public static TestTdsServer StartTestServer(QueryEngine engine, int port = 0, int timeout = 600, bool enableFedAuth = false, bool enableLog = false, [CallerMemberName] string methodName = "")
        {
            return StartServerWithQueryEngine(engine, port, timeout, false, false, methodName);
        }

        public void Dispose() => _endpoint?.Stop();

        public string ConnectionString { get; private set; }

    }
}
