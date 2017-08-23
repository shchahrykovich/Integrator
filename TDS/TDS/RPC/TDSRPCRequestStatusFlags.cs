using System;

namespace TDS.RPC
{
    public enum TDSRPCRequestStatusFlagsByRefValue : byte
    {
        Off = 0,
        On = 1
    }

    public enum TDSRPCRequestStatusFlagsDefaultValue : byte
    {
        Off = 0,
        On = 1
    }

    public enum TDSRPCRequestStatusFlagsEncrypted : byte
    {
        Off = 0,
        On = 1
    }

    public class TDSRPCRequestStatusFlags
    {
        public TDSRPCRequestStatusFlags(byte flags)
        {
            ByRefValue = (TDSRPCRequestOptionFlagsWithRecomp)(flags & 0x1);
            DefaultValue = (TDSRPCRequestOptionFlagsNoMetaData)((flags >> 1) & 0x1);
            Encrypted = (TDSRPCRequestOptionFlagsReuseMetaData)((flags >> 3) & 0x1);
        }

        public TDSRPCRequestOptionFlagsReuseMetaData Encrypted { get; set; }

        public TDSRPCRequestOptionFlagsNoMetaData DefaultValue { get; set; }

        public TDSRPCRequestOptionFlagsWithRecomp ByRefValue { get; set; }
    }
}