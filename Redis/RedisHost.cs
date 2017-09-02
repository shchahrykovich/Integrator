using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Redis
{
    public class RedisHost
    {
        private readonly CancellationToken _token;

        public RedisHost(CancellationToken token)
        {
            _token = token;
        }

        public void Start()
        {
            IPAddress address = IPAddress.Parse("127.0.0.1");
            var listener = new TcpListener(address, 6379);

            listener.Start();

            var parser = new Parser();

            while (_token.IsCancellationRequested)
            {
                var client = listener.AcceptTcpClient();
                using(var stream = client.GetStream())
                {
                    using(var reader = new StreamReader(stream))
                    {
                        var line = reader.ReadLine();
                        var token = parser.Parse(line);
                        switch

                    }
                }
            }
        }
    }
    }
}
