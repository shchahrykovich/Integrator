using System;
using System.Diagnostics;
using Microsoft.SqlServer.TDS;

namespace Runner
{
    [DebuggerDisplay("{Name} {Type}({Size})")]
    public class SqlColumnDefinition
    {
        public String Name { get; set; }
        public TDSDataType Type { get; set; }
        public int Size { get; set; }
    }
}