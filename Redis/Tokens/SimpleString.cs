using System;
using System.Collections.Generic;
using System.Text;

namespace Redis.Tokens
{
    public class SimpleString : Token
    {
        public String Data { get; set; }
    }
}
