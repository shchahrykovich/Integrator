using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Redis
{
    public class RedisHost
    {
        private readonly CancellationToken _token;
        private readonly int _port;
        private readonly GenericRedisEngine _engine;
        private TcpListener _listener;
        private Task _thread;
        private CancellationTokenSource _source;
        private CancellationTokenSource _stopRequest;

        public RedisHost(CancellationToken token, int port, GenericRedisEngine engine)
        {
            _stopRequest = new CancellationTokenSource();
            _source = CancellationTokenSource.CreateLinkedTokenSource(token, _stopRequest.Token);
            _token = _source.Token;
            _port = port;
            _engine = engine;
        }

        public void Start()
        {
            _thread = Task.Run(() => StartInternal());
        }

        public void Stop()
        {
            _stopRequest.Cancel();
            _thread.Wait();
        }

        private void StartInternal()
        {
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();

            try
            {
                while (!_token.IsCancellationRequested)
                {
                    var task = _listener.AcceptTcpClientAsync();
                    task.Wait(_token);
                    Task.Run(() => Process(task.Result));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void Process(TcpClient client)
        {
            using (client)
            {
                var parser = new Parser();
                try
                {
                    using (var stream = client.GetStream())
                    {
                        var redisStreamReader = new RedisStreamReader(stream, _token, 2000000);
                        while (!_token.IsCancellationRequested)
                        {
                            var command = parser.Parse(redisStreamReader);
                            if (null != command)
                            {
                                var result = _engine.Process(command);
                                parser.ConvertToString(result, stream);
                                stream.Flush();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}