using Microsoft.SqlServer.TDS.ColMetadata;

namespace TDS.RPC
{
    public class TDSReturnValueFlags
    {
        /// <summary>
        /// Indicates that the column is nullable
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Indicates that the column is case-sensitive
        /// </summary>
        public bool IsCaseSensitive { get; set; }

        /// <summary>
        /// Indicates that the column updatability
        /// </summary>
        public TDSColumnDataUpdatableFlag Updatable { get; set; }

        /// <summary>
        /// Indicates that the column is identity
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// Indicates that the column is computed
        /// </summary>
        public bool IsComputed { get; set; }

        /// <summary>
        /// Reserved by ODS gateway supporting ODBC
        /// </summary>
        public byte ReservedODBC { get; set; }

        /// <summary>
        /// Indicates that the column is a fixed length CLR type
        /// </summary>
        public bool IsFixedLengthCLR { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TDSReturnValueFlags()
        {
        }

        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="flags">Flags to parse</param>
        public TDSReturnValueFlags(ushort flags)
        {
            // Extract flags per TDS specification, section 2.2.7.4
            IsNullable = ((flags & 0x1) != 0);
            IsCaseSensitive = (((flags >> 1) & 0x1) != 0);
            Updatable = (TDSColumnDataUpdatableFlag)((flags >> 2) & 0x3);
            IsIdentity = (((flags >> 4) & 0x1) != 0);
            IsComputed = (((flags >> 5) & 0x1) != 0);
            ReservedODBC = (byte)((flags >> 6) & 0x3);
            IsFixedLengthCLR = (((flags >> 8) & 0x1) != 0);
        }

        /// <summary>
        /// Serialize flags back into a value
        /// </summary>
        public ushort ToUShort()
        {
            return (ushort)(((ushort)(IsNullable ? 0x1 : 0x0))
                | ((ushort)(IsCaseSensitive ? 0x1 : 0x0)) << 1
                | ((ushort)Updatable) << 2
                | ((ushort)(IsIdentity ? 0x1 : 0x0)) << 4
                | ((ushort)(IsComputed ? 0x1 : 0x0)) << 5
                | ((ushort)ReservedODBC) << 6
                | ((ushort)(IsFixedLengthCLR ? 0x1 : 0x0)) << 8);
        }
    }
}