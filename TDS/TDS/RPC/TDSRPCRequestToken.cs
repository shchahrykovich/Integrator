using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TDS;
using Microsoft.SqlServer.TDS.AllHeaders;
using Microsoft.SqlServer.TDS.Login7;

namespace TDS.RPC
{
    /// <summary>
    /// RPC request
    /// </summary>
    public class TDSRPCRequestToken: TDSPacketToken
    {
        /// <summary>
        /// Inflating constructor
        /// </summary>
        public TDSRPCRequestToken(Stream source)
        {
            // Inflate token
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

            var length = TDSUtilities.ReadUShort(source);
            if (length == 65535)
            {
                ProcID = (TDSRPCRequestTokenProcID)TDSUtilities.ReadUShort(source);
            }
            else
            {
                ProcName = TDSUtilities.ReadString(source, (ushort)(length * 2));
            }

            OptionFlags = new TDSRPCRequestOptionFlags((byte)source.ReadByte());

            var len = source.ReadByte();
            ParamMetaData = TDSUtilities.ReadString(source, (ushort)len);

            StatusFlags = new TDSRPCRequestStatusFlags((byte)source.ReadByte());

            //TDSColumnData

            return true;
        }

        public TDSRPCRequestStatusFlags StatusFlags { get; set; }

        public TDSRPCRequestOptionFlags OptionFlags { get; set; }

        public TDSRPCRequestTokenProcID ProcID { get; set; }

        public string ProcName { get; set; }

        public TDSAllHeadersToken AllHeaders { get; set; }

        public String ParamMetaData { get; set; }

        public override void Deflate(Stream destination)
        {
            throw new NotImplementedException();
        }
    }

    public enum TDSRPCRequestTokenProcID
    {
        Sp_Cursor = 1,
        Sp_CursorOpen = 2,
        Sp_CursorPrepare = 3,
        Sp_CursorExecute = 4,
        Sp_CursorPrepExec = 5,
        Sp_CursorUnprepare = 6,
        Sp_CursorFetch = 7,
        Sp_CursorOption = 8,
        Sp_CursorClose = 9,
        Sp_ExecuteSql = 10,
        Sp_Prepare = 11,
        Sp_Execute = 12,
        Sp_PrepExec = 13,
        Sp_PrepExecRpc = 14,
        Sp_Unprepare = 15
    }
}
