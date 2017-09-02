using System;
using System.Collections.Generic;
using System.Text;

namespace Redis
{
    public enum TokenTypes
    {
        SimpleString,
        Error,
        Integer,
        BulkString,
        Array
    }
}
