using Microsoft.SqlServer.TDS;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.SqlServer.TDS.Row;
using Microsoft.SqlServer.TDS.ColMetadata;

namespace TDS.RPC
{
    public class TDSRPCRequestParameter : TDSPacketToken
    {
        public override bool Inflate(Stream source)
        {
            var len = source.ReadByte();
            if(len == -1)
            {
                return false;
            }
            ParamMetaData = TDSUtilities.ReadString(source, (ushort)(len * 2));

            StatusFlags = new TDSRPCRequestStatusFlags((byte)source.ReadByte());

            DataType = (TDSDataType)source.ReadByte();

            // Dispatch further reading based on the type
            switch (DataType)
            {
                case TDSDataType.Binary:
                case TDSDataType.VarBinary:
                case TDSDataType.Char:
                case TDSDataType.VarChar:
                // The above types are deprecated after TDS 7205.

                case TDSDataType.BitN:
                case TDSDataType.Guid:
                case TDSDataType.IntN:
                case TDSDataType.MoneyN:
                case TDSDataType.FloatN:
                case TDSDataType.DateTimeN:
                    {
                        // Byte length
                        DataTypeSpecific = source.ReadByte();
                        break;
                    }
                case TDSDataType.DateN:
                    {
                        // No details
                        DataTypeSpecific = null;
                        break;
                    }
                case TDSDataType.TimeN:
                case TDSDataType.DateTime2N:
                case TDSDataType.DateTimeOffsetN:
                    {
                        // Scale
                        DataTypeSpecific = source.ReadByte();
                        break;
                    }
                case TDSDataType.DecimalN:
                case TDSDataType.NumericN:
                    {
                        // Read values
                        byte length = (byte)source.ReadByte();
                        byte precision = (byte)source.ReadByte();
                        byte scale = (byte)source.ReadByte();

                        // Decimal data type specific
                        DataTypeSpecific = new TDSDecimalColumnSpecific(length, precision, scale);
                        break;
                    }
                case TDSDataType.BigBinary:
                case TDSDataType.BigVarBinary:
                    {
                        // Short length
                        DataTypeSpecific = TDSUtilities.ReadUShort(source);
                        break;
                    }
                case TDSDataType.BigChar:
                case TDSDataType.BigVarChar:
                case TDSDataType.NChar:
                case TDSDataType.NVarChar:
                    {
                        // SHILOH CHAR types have collation associated with it.
                        TDSShilohVarCharColumnSpecific typedSpecific = new TDSShilohVarCharColumnSpecific();

                        // Read length
                        typedSpecific.Length = TDSUtilities.ReadUShort(source);

                        // Create collation
                        typedSpecific.Collation = new TDSColumnDataCollation();

                        // Read collation
                        typedSpecific.Collation.WCID = TDSUtilities.ReadUInt(source);
                        typedSpecific.Collation.SortID = (byte)source.ReadByte();

                        DataTypeSpecific = typedSpecific;
                        break;
                    }
                case TDSDataType.Text:
                case TDSDataType.NText:
                    {
                        // YukonTextType.Len + YukonTextType.tdsCollationInfo + YukonTextType.cParts
                        // cb = sizeof(LONG) + sizeof(TDSCOLLATION) + sizeof(BYTE);
                        break;
                    }
                case TDSDataType.Image:
                    {
                        // Data length
                        DataTypeSpecific = TDSUtilities.ReadUInt(source);
                        break;
                    }
                case TDSDataType.SSVariant:
                    {
                        // Data length
                        DataTypeSpecific = TDSUtilities.ReadUInt(source);
                        break;
                    }
                case TDSDataType.Udt:
                    {
                        // hr = GetUDTColFmt(pvOwner, dwTimeout);
                        break;
                    }
                case TDSDataType.Xml:
                    {
                        // cb = sizeof(lpColFmt->YukonXmlType.bSchemaPresent);
                        break;
                    }
            }

            Value = TDSRowTokenBase.InflateColumnData(source, DataType, DataTypeSpecific);

            return true;
        }

        public override void Deflate(Stream destination)
        {
            throw new NotImplementedException();
        }

        public String ParamMetaData { get; set; }

        public TDSDataType DataType { get; set; }

        public object DataTypeSpecific { get; set; }

        public object Value { get; set; }

        public TDSRPCRequestStatusFlags StatusFlags { get; set; }
    }
}
