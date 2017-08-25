using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TDS;
using Microsoft.SqlServer.TDS.AllHeaders;
using Microsoft.SqlServer.TDS.Login7;
using Microsoft.SqlServer.TDS.ColMetadata;
using Microsoft.SqlServer.TDS.Row;

namespace TDS.RPC
{
    /// <summary>
    /// RPC request
    /// </summary>
    public class TDSRPCRequestToken: TDSPacketToken
    {
        public TDSRPCRequestToken()
        {
            Parameters = new List<TDSRPCRequestParameter>();
        }

        /// <summary>
        /// Inflating constructor
        /// </summary>
        public TDSRPCRequestToken(Stream source)
        {
            Parameters = new List<TDSRPCRequestParameter>();
            Inflate(source);
        }

        /// <summary>
        /// Inflate the token
        /// NOTE: This operation is not continuable and assumes that the entire token is available in the stream
        /// </summary>
        /// <param name="source">Stream to inflate the token from</param>
        /// <returns>TRUE if inflation is complete</returns>
        public override bool Inflate(Stream source)
        {
            AllHeaders = new TDSAllHeadersToken();
           
            // Inflate all headers
            if (!AllHeaders.Inflate(source))
            {
                // Failed to inflate headers
                throw new ArgumentException("Failed to inflate all headers");
            }

            var lengthProcName = TDSUtilities.ReadUShort(source);
            if (lengthProcName == 65535)
            {
                ProcID = (TDSRPCRequestTokenProcID)TDSUtilities.ReadUShort(source);
            }
            else
            {
                ProcName = TDSUtilities.ReadString(source, (ushort)(lengthProcName * 2));
            }

            OptionFlags = new TDSRPCRequestOptionFlags((byte)TDSUtilities.ReadUShort(source));

            TDSRPCRequestParameter p = new TDSRPCRequestParameter();
            while(p.Inflate(source))
            {
                Parameters.Add(p);
                p = new TDSRPCRequestParameter();
            }

            return true;
        }

        public TDSRPCRequestOptionFlags OptionFlags { get; set; }

        public TDSRPCRequestTokenProcID ProcID { get; set; }

        public string ProcName { get; set; }

        public TDSAllHeadersToken AllHeaders { get; set; }
        public List<TDSRPCRequestParameter> Parameters { get; set; }

        public override void Deflate(Stream destination)
        {
            throw new NotImplementedException();
        }
    }
}
