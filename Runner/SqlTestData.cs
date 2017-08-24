using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Newtonsoft.Json;

namespace Runner
{
    public class SqlTestData
    {
        public String Query { get; set; }
        public SqlRecord[] Records { get; set; }

        [JsonProperty("rpc-count")]
        public ulong RpcCount { get; set; }

        [JsonProperty("rpc-scalar")]
        public ulong RpcScalar { get; set; }
    }

    public class SqlRecord
    {
        public SqlColumn[] SqlColumn { get; set; }
    }

    public class SqlColumn
    {
        public String Name { get; set; }
        public String Value { get; set; }
        public String Type { get; set; }
    }
}
