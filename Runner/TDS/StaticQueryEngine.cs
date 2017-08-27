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

        private readonly Dictionary<String, SqlStub> _stubs;

        public StaticQueryEngine(TDSServerArguments arguments) : base(arguments)
        {
            _stubs = new Dictionary<string, SqlStub>(StringComparer.OrdinalIgnoreCase);
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
                        if (_stubs.ContainsKey(sql))
                        {
                            var result = _stubs[sql];
                            if (result.GetRecords().Any())
                            {
                                var tokens = ReturnTable(result);

                                // Create DONE token
                                TDSDoneToken doneToken =
                                    new TDSDoneToken(TDSDoneTokenStatusType.Final | TDSDoneTokenStatusType.Count,
                                        TDSDoneTokenCommandType.Select, (ulong) result.GetRecords().Count());
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
                if (_stubs.ContainsKey(text))
                {
                    var result = _stubs[text];

                    if (result.GetRecords().Any())
                    {
                        var tokens = ReturnTable(result);

                        // Create DONE token
                        TDSDoneToken doneToken =
                            new TDSDoneToken(TDSDoneTokenStatusType.Final | TDSDoneTokenStatusType.Count,
                                TDSDoneTokenCommandType.Select, (ulong) result.GetRecords().Count());
                        tokens.Add(doneToken);

                        // Log response
                        TDSUtilities.Log(Log, "Response", doneToken);

                        // Serialize tokens into the message
                        return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response, tokens.ToArray()));
                    }
                    else
                    {
                        TDSDoneInProcToken doneIn = 
                            new TDSDoneInProcToken(TDSDoneTokenStatusType.Count, TDSDoneTokenCommandType.DoneInProc, result.RpcCount);

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

        private List<TDSPacketToken> ReturnTable(SqlStub result)
        {
            List<TDSPacketToken> tokens = new List<TDSPacketToken>();

            var records = result.GetRecords().ToArray();
            if (0 < records.Length)
            {
                TDSColMetadataToken metadataToken = new TDSColMetadataToken();
                tokens.Add(metadataToken);

                foreach (var columnDefinition in result.GetColumns())
                {
                    TDSColumnData column = new TDSColumnData();

                    column.DataType = columnDefinition.Type;
                    column.Flags.Updatable = TDSColumnDataUpdatableFlag.ReadOnly;

                    if (columnDefinition.Type == TDSDataType.NVarChar)
                    {
                        column.DataTypeSpecific = new TDSShilohVarCharColumnSpecific((ushort)columnDefinition.Size,
                            new TDSColumnDataCollation(13632521, 52));
                    }
                    else
                    {
                        column.DataTypeSpecific = (byte) columnDefinition.Size;
                        column.Flags.IsComputed = true;
                        column.Flags.IsNullable =
                            true; // TODO: Must be nullable, otherwise something is wrong with SqlClient
                    }

                    column.Name = columnDefinition.Name;
                    metadataToken.Columns.Add(column);
                }

                foreach (var recordData in records)
                {
                    TDSRowToken rowToken = new TDSRowToken(metadataToken);

                    foreach (var value in recordData)
                    {
                        rowToken.Data.Add(value);
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

            if (_stubs.ContainsKey(lowerBatchText))
            {
                List<TDSPacketToken> tokens = new List<TDSPacketToken>();
                TDSColMetadataToken metadataToken = new TDSColMetadataToken();
                tokens.Add(metadataToken);

                var result = _stubs[lowerBatchText];

                tokens.AddRange(ReturnTable(result));

                // Create DONE token
                TDSDoneToken doneToken = new TDSDoneToken(TDSDoneTokenStatusType.Final | TDSDoneTokenStatusType.Count, 
                    TDSDoneTokenCommandType.Select, (ulong)result.GetRecords().Count());
                tokens.Add(doneToken);

                // Log response
                TDSUtilities.Log(Log, "Response", doneToken);

                // Serialize tokens into the message
                return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response, tokens.ToArray()));
            }

            return base.CreateQueryResponse(session, batchRequest);
        }

        public void AddStub(SqlStub stub)
        {
            _stubs.Add(stub.Query, stub);
        }
    }
}
