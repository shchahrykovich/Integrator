using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.SqlServer.TDS;
using Microsoft.SqlServer.TDS.AllHeaders;

namespace TDS.Transactions
{
    public class TDSTransMgrReqToken : TDSPacketToken
    {
        public TDSTransMgrReqToken(Stream source)
        {
            Inflate(source);
        }

        public override bool Inflate(Stream source)
        {
            AllHeaders = new TDSAllHeadersToken();

            // Inflate all headers
            if (!AllHeaders.Inflate(source))
            {
                // Failed to inflate headers
                throw new ArgumentException("Failed to inflate all headers");
            }

            RequestType = (TDSTransMgrReqRequestType)TDSUtilities.ReadUShort(source);
            switch (RequestType)
            {
                case TDSTransMgrReqRequestType.TM_GET_DTC_ADDRESS:
                case TDSTransMgrReqRequestType.TM_PROPAGATE_XACT:
                    var lenPayload = TDSUtilities.ReadUShort(source);
                    byte[] arrayPayload = new byte[lenPayload];
                    source.Read(arrayPayload, 0, lenPayload);
                    RequestPayload = arrayPayload;
                    break;
                case TDSTransMgrReqRequestType.TM_BEGIN_XACT:
                    TransactionIsolationLevel = (TransactionIsolationLevelType)source.ReadByte();
                    var len = source.ReadByte();
                    byte[] array = new byte[len];
                    source.Read(array, 0, len);
                    XactName = array;
                    break;
            }

            return true;
        }

        public byte[] RequestPayload { get; set; }

        public byte[] XactName { get; set; }

        public TransactionIsolationLevelType TransactionIsolationLevel { get; set; }

        public TDSTransMgrReqRequestType RequestType { get; set; }

        public TDSAllHeadersToken AllHeaders { get; set; }

        public override void Deflate(Stream destination)
        {
            throw new NotImplementedException();
        }
    }
}
