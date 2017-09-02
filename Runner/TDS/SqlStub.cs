using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using Microsoft.SqlServer.TDS;

namespace Runner.TDS
{
    [DebuggerDisplay("{FilePath} - {Query}")]
    public class SqlStub : Stub
    {
        private static char[] ColumnSeparator = new[] {'\t'};

        private List<SqlColumnDefinition> _columns;

        private List<Object[]> _records;

        public String Query { get; set; }

        public String Table { get; set; }

        public ulong Scalar { get; set; }

        public Dictionary<String, Object> Parameters { get; set; }

        public IEnumerable<Object[]> GetRecords()
        {
            if (null == _records)
            {
                try
                {
                    if (null == Table)
                    {
                        _records = new List<object[]>();
                    }
                    else
                    {
                        _records = new List<object[]>(GetRecordsInternal());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
                
            }

            return _records;
        }

        private IEnumerable<Object[]> GetRecordsInternal()
        {
            var columns = GetColumns().ToArray();
            foreach (var row in Table.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                var result = new Object[columns.Length];
                var record = row.Split(ColumnSeparator).ToArray();
                for (int i = 0; i < record.Length; i++)
                {
                    try
                    {
                        result[i] = GetValue(columns[i], record[i]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Can't parse " + columns[i].Name + " (" + columns[i].Type.ToString() + ")");
                        Console.WriteLine(e);
                        throw;
                    }
                    
                }
                yield return result;
            }
        }

        private static Object GetValue(SqlColumnDefinition column, string value)
        {
            if (0 == String.Compare(value, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            switch (column.Type)
            {
                case TDSDataType.Int4:
                case TDSDataType.Int1:
                case TDSDataType.Int2:
                case TDSDataType.IntN:
                case TDSDataType.Int8:
                {
                    switch (column.Size)
                    {
                        case 1:
                        {
                            return (byte) int.Parse(value);
                        }
                        case 2:
                        {
                            return (short) int.Parse(value);
                        }
                        case 4:
                        {
                            return int.Parse(value);
                        }
                        case 8:
                        {
                            return long.Parse(value);
                        }
                        default:
                            throw new NotImplementedException("Can't convert size - " + column.Size);
                    }
                }
                case TDSDataType.BigVarBinary:
                {
                    var array = value.Substring(2).ToCharArray();
                    return Convert.FromBase64CharArray(array, 0, array.Length);
                }
                case TDSDataType.Guid:
                {
                    return Guid.Parse(value);
                }
                case TDSDataType.Xml:
                {
                    return value;
                }
                case TDSDataType.MoneyN:
                case TDSDataType.DecimalN:
                {
                    return decimal.Parse(value);
                }
                case TDSDataType.Bit:
                {
                    if (value == "0")
                    {
                        return false;
                    }
                    else if (value == "1")
                    {
                        return true;
                    }
                    else
                    {
                        return bool.Parse(value);
                    }
                }
                case TDSDataType.NVarChar:
                {
                    return value;
                }
                case TDSDataType.DateTime:
                case TDSDataType.DateTime2N:
                    {
                    return DateTime.Parse(value);
                }
                default:
                    throw new NotImplementedException("Can't parse type " + column.Type);
            }
        }

        public IEnumerable<SqlColumnDefinition> GetColumns()
        {
            if (null == _columns)
            {
                _columns = new List<SqlColumnDefinition>(GetColumnsInternal());
            }
            return _columns;
        }

        private IEnumerable<SqlColumnDefinition> GetColumnsInternal()
        {
            if (!String.IsNullOrEmpty(Table))
            {
                var header = Table.Split("\r\n").First();
                foreach (var rawColumnName in header.Split(ColumnSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (rawColumnName.Contains("/"))
                    {
                        var parts = rawColumnName.Split("/");
                        var columnName = parts[0];
                        var type = parts[1];


                        var columnDef = new SqlColumnDefinition
                        {
                            Name = columnName
                        };
                        InjectTDSType(columnDef, type);

                        yield return columnDef;
                    }
                    else
                    {
                        SqlColumnDefinition columnDef = new SqlColumnDefinition();
                        if (!TryGuess(columnDef, rawColumnName))
                        {
                            columnDef = new SqlColumnDefinition
                            {
                                Name = rawColumnName,
                                Type = TDSDataType.NVarChar,
                                Size = 4000
                            };
                        }
                        yield return columnDef;
                    }
                }
            }
        }

        private bool TryGuess(SqlColumnDefinition column, string columnName)
        {
            if (columnName.StartsWith("date", StringComparison.OrdinalIgnoreCase) ||
                columnName.EndsWith("date", StringComparison.OrdinalIgnoreCase))
            {
                column.Name = columnName;
                column.Type = TDSDataType.DateTime2N;
                column.Size = 7;

                return true;
            }


            if (columnName.StartsWith("guid", StringComparison.OrdinalIgnoreCase) ||
                columnName.EndsWith("guid", StringComparison.OrdinalIgnoreCase))
            {
                column.Name = columnName;
                column.Type = TDSDataType.Guid;
                column.Size = 16;

                return true;
            }

            if (columnName.StartsWith("id", StringComparison.OrdinalIgnoreCase) ||
                columnName.EndsWith("id", StringComparison.OrdinalIgnoreCase))
            {
                column.Name = columnName;
                column.Type = TDSDataType.IntN;
                column.Size = 4;

                return true;
            }

            return false;
        }

        private void InjectTDSType(SqlColumnDefinition column, string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "int":
                {
                    column.Type = TDSDataType.IntN;
                    column.Size = 4;
                    break;
                }
                case "bit":
                {
                    column.Type = TDSDataType.Bit;
                    break;
                }
                case "tinyint":
                {
                    column.Type = TDSDataType.IntN;
                    column.Size = 1;
                    break;
                }
                case "bigint":
                {
                    column.Type = TDSDataType.IntN;
                    column.Size = 8;
                    break;
                }
                case "datetime":
                {
                    column.Type = TDSDataType.DateTime2N;
                    column.Size = 7;
                    break;
                }
                case "smallint":
                {
                    column.Type = TDSDataType.Int2;
                    column.Size = 2;
                    break;
                }
                case "nvarchar":
                {
                    column.Type = TDSDataType.NVarChar;
                    column.Size = 1000;
                    break;
                }
                case "varbinary":
                {
                    column.Type = TDSDataType.BigVarBinary;
                    column.Size = 1000;
                    break;
                }
                case "money":
                {
                    column.Type = TDSDataType.MoneyN;
                    column.Size = 8;
                    break;
                }
                case "guid":
                {
                    column.Type = TDSDataType.Guid;
                    column.Size = 16;
                    break;
                }
                case "decimal":
                {
                    column.Type = TDSDataType.DecimalN;
                    break;
                }
                case "xml":
                {
                    column.Type = TDSDataType.Xml;
                    column.Size = 4000;
                    break;
                }
                default:
                    throw new NotImplementedException("Can't understand type " + type + ".");
            }
        }
    }
}
