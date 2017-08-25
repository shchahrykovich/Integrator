using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.TDS;
using Microsoft.SqlServer.TDS.ColMetadata;
using Microsoft.SqlServer.TDS.Done;
using Microsoft.SqlServer.TDS.EndPoint;
using Microsoft.SqlServer.TDS.ReturnStatus;
using Microsoft.SqlServer.TDS.Row;
using Microsoft.SqlServer.TDS.Servers;
using Microsoft.SqlServer.TDS.SQLBatch;
using TDS.RPC;

namespace Runner
{
    public class StaticQueryEngine: QueryEngine
    {
        public String Name { get; set; }

        private readonly Dictionary<String, SqlTestData> _data;

        public StaticQueryEngine(TDSServerArguments arguments) : base(arguments)
        {
            _data = new Dictionary<string, SqlTestData>(StringComparer.OrdinalIgnoreCase);
        }

        public override TDSMessageCollection ExecuteRPC(ITDSServerSession session, TDSMessage message)
        {
            TDSRPCRequestToken rpc = message[0] as TDSRPCRequestToken;
            TDSDoneToken done = new TDSDoneToken(TDSDoneTokenStatusType.Final, TDSDoneTokenCommandType.Done, 0);

            if (String.IsNullOrEmpty(rpc.ProcName))
            {
                PrintLog("Executing - " + rpc.ProcID.ToString());
                foreach (var parameter in rpc.Parameters)
                {
                    PrintLog("Parameter " + parameter.ParamMetaData);
                }
                if (rpc.ProcID == TDSRPCRequestTokenProcID.Sp_ExecuteSql)
                {
                    var sqlQuery = rpc.Parameters.First();
                    if (sqlQuery.DataType == TDSDataType.NVarChar)
                    {
                        var sql = sqlQuery.Value.ToString();
                        PrintLog("Query - " + sql);
                        if (_data.ContainsKey(sql))
                        {
                            var result = _data[sql];
                            if (null != result.Records)
                            {
                                var tokens = ReturnTable(result);

                                // Create DONE token
                                TDSDoneToken doneToken =
                                    new TDSDoneToken(TDSDoneTokenStatusType.Final | TDSDoneTokenStatusType.Count,
                                        TDSDoneTokenCommandType.Select, (ulong) result.Records.Length);
                                tokens.Add(doneToken);

                                // Log response
                                TDSUtilities.Log(Log, "Response", doneToken);

                                // Serialize tokens into the message
                                return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response,
                                    tokens.ToArray()));
                            }
                        }
                    }
                }
            }
            else
            {
                var text = rpc.ProcName.ToLowerInvariant();
                PrintLog("Executing - " + text);
                if (_data.ContainsKey(text))
                {
                    var result = _data[text];

                    if (null != result.Records)
                    {
                        var tokens = ReturnTable(result);

                        // Create DONE token
                        TDSDoneToken doneToken =
                            new TDSDoneToken(TDSDoneTokenStatusType.Final | TDSDoneTokenStatusType.Count,
                                TDSDoneTokenCommandType.Select, (ulong) result.Records.Length);
                        tokens.Add(doneToken);

                        // Log response
                        TDSUtilities.Log(Log, "Response", doneToken);

                        // Serialize tokens into the message
                        return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response, tokens.ToArray()));
                    }
                    else
                    {
                        TDSDoneInProcToken doneIn =
                            new TDSDoneInProcToken(TDSDoneTokenStatusType.Count, TDSDoneTokenCommandType.DoneInProc,
                                result.RpcCount);

                        return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response, doneIn));
                    }
                }
            }
            return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response, done));
        }

        private void PrintLog(String message)
        {
            Console.WriteLine(" --- " + Name + " --- " + message);
        }

        private List<TDSPacketToken> ReturnTable(SqlTestData result)
        {
            List<TDSPacketToken> tokens = new List<TDSPacketToken>();
            

            if (0 < result.Records.Length)
            {
                TDSColMetadataToken metadataToken = new TDSColMetadataToken();
                tokens.Add(metadataToken);

                foreach (var columnData in result.Records.First().SqlColumn)
                {
                    TDSColumnData column = new TDSColumnData();

                    if (columnData.Type == "string")
                    {
                        column.DataType = TDSDataType.NVarChar;
                        column.DataTypeSpecific =
                            new TDSShilohVarCharColumnSpecific(10000,
                                new TDSColumnDataCollation(13632521, 52));
                        column.Flags.Updatable = TDSColumnDataUpdatableFlag.ReadOnly;
                    }
                    else if (columnData.Type == "int")
                    {
                        column.DataType = TDSDataType.IntN;
                        column.DataTypeSpecific = (byte) 4;
                        column.Flags.Updatable = TDSColumnDataUpdatableFlag.ReadOnly;
                        column.Flags.IsComputed = true;
                        column.Flags.IsNullable =
                            true; // TODO: Must be nullable, otherwise something is wrong with SqlClient
                    }
                    else if (columnData.Type == "byte")
                    {
                        column.DataType = TDSDataType.IntN;
                        column.DataTypeSpecific = (byte) 1;
                        column.Flags.Updatable = TDSColumnDataUpdatableFlag.ReadOnly;
                        column.Flags.IsComputed = true;
                        column.Flags.IsNullable =
                            true; // TODO: Must be nullable, otherwise something is wrong with SqlClient
                    }

                    column.Name = columnData.Name;
                    metadataToken.Columns.Add(column);
                }

                foreach (var recordData in result.Records)
                {
                    // Log response
                    TDSUtilities.Log(Log, "Response", metadataToken);

                    // Prepare result data
                    TDSRowToken rowToken = new TDSRowToken(metadataToken);

                    foreach (var columnData in recordData.SqlColumn)
                    {
                        if (columnData.Type == "string")
                        {
                            rowToken.Data.Add(columnData.Value);
                        }
                        else if (columnData.Type == "int")
                        {
                            rowToken.Data.Add(Int32.Parse(columnData.Value));
                        }
                        else if (columnData.Type == "byte")
                        {
                            rowToken.Data.Add((byte)Int32.Parse(columnData.Value));
                        }

                        // Log response
                        TDSUtilities.Log(Log, "Response", rowToken);
                    }
                    tokens.Add(rowToken);
                }
            }
            return tokens;
        }

        protected override TDSMessageCollection CreateQueryResponse(ITDSServerSession session, TDSSQLBatchToken batchRequest)
        {
            string lowerBatchText = batchRequest.Text.ToLowerInvariant();
            PrintLog(lowerBatchText);

            if (_data.ContainsKey(lowerBatchText))
            {
                List<TDSPacketToken> tokens = new List<TDSPacketToken>();
                TDSColMetadataToken metadataToken = new TDSColMetadataToken();
                tokens.Add(metadataToken);

                var result = _data[lowerBatchText];

                tokens.AddRange(ReturnTable(result));

                // Create DONE token
                TDSDoneToken doneToken = new TDSDoneToken(TDSDoneTokenStatusType.Final | TDSDoneTokenStatusType.Count, TDSDoneTokenCommandType.Select, (ulong)result.Records.Length);
                tokens.Add(doneToken);

                // Log response
                TDSUtilities.Log(Log, "Response", doneToken);

                // Serialize tokens into the message
                return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response, tokens.ToArray()));
            }

            return base.CreateQueryResponse(session, batchRequest);
        }

        public void AddTestData(SqlTestData sqlTestData)
        {
            _data.Add(sqlTestData.Query.ToLowerInvariant(), sqlTestData);
        }
    }
}
