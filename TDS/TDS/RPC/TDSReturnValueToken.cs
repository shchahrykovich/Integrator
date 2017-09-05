using Microsoft.SqlServer.TDS;
using Microsoft.SqlServer.TDS.ColMetadata;
using System;
using System.IO;

namespace TDS.RPC
{
    public class TDSReturnValueToken : TDSPacketToken
    {
        public ushort ParamOrdinal { get; set; }

        public String ParamName { get; set; }

        public TDSReturnValueStatus Status { get; set; }
        
        public uint UserType { get; set; }

        public TDSReturnValueFlags Flags { get; set; }

        public TDSDataType DataType { get; set; }

        public object DataTypeSpecific { get; set; }

        public object Value { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TDSReturnValueToken()
        {
            Flags = new TDSReturnValueFlags();
        }

        /// <summary>
        /// Inflate the token
        /// NOTE: This operation is not continuable and assumes that the entire token is available in the stream
        /// </summary>
        /// <param name="source">Stream to inflate the token from</param>
        /// <returns>TRUE if inflation is complete</returns>
        public override bool Inflate(Stream source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deflate the token
        /// </summary>
        /// <param name="destination">Stream to deflate token to</param>
        public override void Deflate(Stream destination)
        {
            destination.WriteByte((byte)TDSTokenType.ReturnValue);

            TDSUtilities.WriteUShort(destination, ParamOrdinal);

            destination.WriteByte((byte)ParamName.Length);
            TDSUtilities.WriteString(destination, ParamName);

            destination.WriteByte((byte)Status);

            TDSUtilities.WriteUInt(destination, UserType);

            TDSUtilities.WriteUShort(destination, Flags.ToUShort());

            // Write type
            destination.WriteByte((byte)DataType);

            // Dispatch further writing based on the type
            switch (DataType)
            {
                case TDSDataType.Binary:
                case TDSDataType.VarBinary:
                case TDSDataType.Char:
                case TDSDataType.VarChar:
                case TDSDataType.BitN:
                case TDSDataType.Guid:
                case TDSDataType.IntN:
                case TDSDataType.MoneyN:
                case TDSDataType.FloatN:
                case TDSDataType.DateTimeN:
                    {
                        // Byte length
                        destination.WriteByte((byte)DataTypeSpecific);
                        break;
                    }
                case TDSDataType.DateN:
                    {
                        // No details
                        break;
                    }
                case TDSDataType.TimeN:
                case TDSDataType.DateTime2N:
                case TDSDataType.DateTimeOffsetN:
                    {
                        // Scale
                        destination.WriteByte((byte)DataTypeSpecific);
                        break;
                    }
                case TDSDataType.DecimalN:
                case TDSDataType.NumericN:
                    {
                        // Cast to type-specific information
                        TDSDecimalColumnSpecific typeSpecific = DataTypeSpecific as TDSDecimalColumnSpecific;

                        // Write values
                        destination.WriteByte(typeSpecific.Length);
                        destination.WriteByte(typeSpecific.Precision);
                        destination.WriteByte(typeSpecific.Scale);
                        break;
                    }
                case TDSDataType.BigBinary:
                case TDSDataType.BigVarBinary:
                    {
                        // Short length
                        TDSUtilities.WriteUShort(destination, (ushort)DataTypeSpecific);
                        break;
                    }
                case TDSDataType.BigChar:
                case TDSDataType.BigVarChar:
                case TDSDataType.NChar:
                case TDSDataType.NVarChar:
                    {
                        // Cast to type specific information
                        TDSShilohVarCharColumnSpecific typedSpecific = DataTypeSpecific as TDSShilohVarCharColumnSpecific;

                        // Write length
                        TDSUtilities.WriteUShort(destination, typedSpecific.Length);

                        // Write collation
                        TDSUtilities.WriteUInt(destination, typedSpecific.Collation.WCID);
                        destination.WriteByte(typedSpecific.Collation.SortID);
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
                        // Integer length
                        TDSUtilities.WriteUInt(destination, (uint)DataTypeSpecific);
                        break;
                    }
                case TDSDataType.SSVariant:
                    {
                        // Data length
                        TDSUtilities.WriteUInt(destination, (uint)DataTypeSpecific);
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

            Microsoft.SqlServer.TDS.Row.TDSRowTokenBase.DeflateTypeVarByte(destination, DataType, Value, DataTypeSpecific);
        }
    }
}