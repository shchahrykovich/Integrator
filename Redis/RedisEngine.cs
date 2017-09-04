using System;
using System.Linq;
using System.Net.Http.Headers;
using Redis.Tokens;

namespace Redis
{
    public class GenericRedisEngine
    {
        private readonly int _port;

        private static RedisToken OkResponse = new SimpleStringRedisToken
        {
            Data = "OK"
        };

        private static String ServerInfo = @"# Server
redis_version:3.2.100

# Replication
role:master

# Cluster
cluster_enabled:0

# Keyspace";

        public GenericRedisEngine(int port)
        {
            _port = port;
        }

        public RedisToken Process(RedisToken command)
        {
            if (command is ArrayRedisToken array)
            {
                var text = String.Join(" ", array.Items.Select(i => i.GetData())).ToLowerInvariant();
                if (text == "config get timeout")
                {
                    var result = new ArrayRedisToken(2);
                    result.Add(new SimpleStringRedisToken {Data = "timeout"});
                    result.Add(new SimpleStringRedisToken {Data = "0"});
                    return result;
                }
                else if (text == "config get slave-read-only")
                {
                    var result = new ArrayRedisToken(2);
                    result.Add(new SimpleStringRedisToken {Data = "slave-read-only"});
                    result.Add(new SimpleStringRedisToken {Data = "yes"});
                    return result;
                }
                else if (text == "config get databases")
                {
                    var result = new ArrayRedisToken(2);
                    result.Add(new SimpleStringRedisToken {Data = "databases"});
                    result.Add(new SimpleStringRedisToken {Data = "1"});
                    return result;
                }
                else if (text.StartsWith("echo"))
                {
                    return array.Items.ElementAt(1);
                }
                else if (text == "info replication")
                {
                    return new BulkStringRedisToken
                    {
                        Data = @"# Replication
role:master
connected_slaves:0"
                    };
                }
                else if (text == "info server")
                {
                    return new BulkStringRedisToken
                    {
                        Data = @"# Server
redis_version:3.2.100"
                    };
                }
                else if (text == "info")
                {
                    return new BulkStringRedisToken
                    {
                        Data = ServerInfo.Replace("{port}", _port.ToString())
                    };
                }
                else if (text.StartsWith("client setname "))
                {
                    return OkResponse;
                }
                else if (text == "cluster nodes")
                {
                    return new ErrorRedisToken
                    {
                        Data = "ERR This instance has cluster support disabled"
                    };
                }
                else if (text == "ping")
                {
                    return new SimpleStringRedisToken
                    {
                        Data = "PONG"
                    };
                }
                else if (text.StartsWith("get "))
                {
                    return new BulkStringRedisToken();
                }
                else if (text.StartsWith("subscribe"))
                {
                    var result = new ArrayRedisToken(3);
                    result.Add(new SimpleStringRedisToken { Data = "subscribe" });
                    result.Add(array.Items.ElementAt(1));
                    result.Add(new IntegerRedisToken
                    {
                        Data = 1
                    });
                    return result;
                }
                else
                {
                    return OkResponse;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}