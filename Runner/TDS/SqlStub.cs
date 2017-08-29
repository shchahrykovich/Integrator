using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TDS;

namespace Runner
{
    [DebuggerDisplay("{FileName} - {Query}")]
    public class SqlStub
    {
        private static char[] ColumnSeparator = new[] {'\t'};

        private List<SqlColumnDefinition> _columns;

        private List<Object[]> _records;

        public String Query { get; set; }

        public String Table { get; set; }

        public ulong RpcCount { get; set; }

        [YamlDotNet.Serialization.YamlIgnore]
        public string FileName { get; set; }

        public IEnumerable<Object[]> GetRecords()
        {
            if (null == _records)
            {
                _records = new List<object[]>(GetRecordsInternal());
            }

            return _records;
        }

        private IEnumerable<Object[]> GetRecordsInternal()
        {
            var columns = GetColumns().ToArray();
            foreach (var row in Table.Split("\n", StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                var result = new Object[columns.Length];
                var record = row.Split(ColumnSeparator).ToArray();
                for (int i = 0; i < record.Length; i++)
                {

                    var column = columns[i];
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
                                    result[i] = (byte) int.Parse(record[i]);
                                    break;
                                }
                                case 2:
                                {
                                    result[i] = (short) int.Parse(record[i]);
                                    break;
                                }
                                case 4:
                                {
                                    result[i] = int.Parse(record[i]);
                                    break;
                                }
                                case 8:
                                {
                                    result[i] = long.Parse(record[i]);
                                    break;
                                }
                                    default:
                                        throw new NotImplementedException("Can't convert size - " + column.Size);
                                }
                                break;
                        }
                        case TDSDataType.NVarChar:
                        {
                            result[i] = record[i];
                            break;
                        }
                        default:
                            throw new NotImplementedException("Can't parse type " + column.Type);
                    }
                }
                yield return result;
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
                var header = Table.Split("\n").First();
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
                case "tinyint":
                {
                    column.Type = TDSDataType.IntN;
                    column.Size = 1;
                    break;
                }
                default:
                    throw new NotImplementedException("Can't understand type " + type);
            }
        }
    }
}
