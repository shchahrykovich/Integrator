using System;
using System.Collections.Generic;
using System.Text;

namespace TDS.RPC
{

    public enum TDSRPCRequestOptionFlagsWithRecomp : byte
    {
        Off = 0,
        On = 1
    }

    public enum TDSRPCRequestOptionFlagsNoMetaData : byte
    {
        Off = 0,
        On = 1
    }

    public enum TDSRPCRequestOptionFlagsReuseMetaData : byte
    {
        Off = 0,
        On = 1
    }

    public class TDSRPCRequestOptionFlags
    {
        public TDSRPCRequestOptionFlags(byte flags)
        {
            WithRecomp = (TDSRPCRequestOptionFlagsWithRecomp)(flags & 0x1);
            NoMetaData = (TDSRPCRequestOptionFlagsNoMetaData)((flags >> 1) & 0x1);
            ReuseMetaData = (TDSRPCRequestOptionFlagsReuseMetaData)((flags >> 2) & 0x1);
        }

        public TDSRPCRequestOptionFlagsReuseMetaData ReuseMetaData { get; set; }

        public TDSRPCRequestOptionFlagsNoMetaData NoMetaData { get; set; }

        public TDSRPCRequestOptionFlagsWithRecomp WithRecomp { get; set; }
    }
}
