using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TDS;
using Microsoft.SqlServer.TDS.ColMetadata;
using Microsoft.SqlServer.TDS.Done;
using Microsoft.SqlServer.TDS.EndPoint;
using Microsoft.SqlServer.TDS.Row;
using Microsoft.SqlServer.TDS.Servers;
using Microsoft.SqlServer.TDS.SQLBatch;
using TDS.RPC;

namespace Runner.TDS
{
    public class StaticQueryEngine: QueryEngine
    {
        public String Name { get; set; }

        private readonly Dictionary<String, List<SqlStub>> _stubs;

        public StaticQueryEngine(TDSServerArguments arguments) : base(arguments)
        {
            _stubs = new Dictionary<string, List<SqlStub>>(StringComparer.OrdinalIgnoreCase);
        }

        private void PrintMissignQuery(string sql, List<TDSRPCRequestParameter> parameters)
        {
            lock (_stubs)
            {
                Console.WriteLine($"--------------{Name}---------------------");
                Console.WriteLine(sql);
                
                if (null != parameters)
                {
                    foreach (var parameter in parameters)
                    {
                        if (null != parameter.ParamMetaData && "@RETURN_VALUE" != parameter.ParamMetaData)
                        {
                            Console.WriteLine(parameter.ParamMetaData + " = " + parameter.Value);
                        }
                    }
                }
            }
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
                    else if (columnDefinition.Type == TDSDataType.DateTime2N || 
                             columnDefinition.Type == TDSDataType.DateTime)
                    {
                        column.DataTypeSpecific = (byte)columnDefinition.Size;
                        column.Flags.IsComputed = true;
                        column.Flags.IsNullable =
                            true; // TODO: Must be nullable, otherwise something is wrong with SqlClient
                    }
                    else if (columnDefinition.Type == TDSDataType.Bit)
                    {
                    }
                    else if (columnDefinition.Type == TDSDataType.BigVarBinary)
                    {
                        column.DataTypeSpecific = (ushort)columnDefinition.Size;
                    }
                    else if (columnDefinition.Type == TDSDataType.Int2)
                    {
                        column.DataTypeSpecific = (byte)columnDefinition.Size;
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

        private SqlStub TryGet(string sql, List<TDSRPCRequestParameter> sqlParameters)
        {
            var key = sql.Trim();
            if (_stubs.ContainsKey(key))
            {
                var stubs = _stubs[key];
                foreach (var stub in stubs)
                {
                    if (null == stub.Parameters)
                    {
                        return stub;
                    }
                    else
                    {
                        var found = true;
                        foreach (var stubParameter in stub.Parameters)
                        {
                            var param = sqlParameters.SingleOrDefault(s => s.ParamMetaData == "@" + stubParameter.Key);
                            if (null != param)
                            {
                                if (param.Value.ToString() != stubParameter.Value.ToString())
                                {
                                    found = false;
                                    break;
                                }
                            }
                        }
                        if (found)
                        {
                            return stub;
                        }
                    }
                }
            }

            return null;
        }

        public override TDSMessageCollection ExecuteRPC(ITDSServerSession session, TDSMessage message)
        {
            TDSRPCRequestToken rpc = message[0] as TDSRPCRequestToken;
            TDSDoneToken done = new TDSDoneToken(TDSDoneTokenStatusType.Final, TDSDoneTokenCommandType.Done, 0);

            if (String.IsNullOrEmpty(rpc.ProcName))
            {
                if (rpc.ProcID == TDSRPCRequestTokenProcID.Sp_ExecuteSql)
                {
                    var sqlQuery = rpc.Parameters.First();
                    if (sqlQuery.DataType == TDSDataType.NVarChar)
                    {
                        var sql = sqlQuery.Value.ToString();
                        var result = TryGet(sql, rpc.Parameters);
                        if (null != result)
                        {
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
                            else
                            {
                                TDSDoneInProcToken doneIn =
                                    new TDSDoneInProcToken(TDSDoneTokenStatusType.Count, TDSDoneTokenCommandType.DoneInProc,
                                        result.Scalar);

                                return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response, doneIn));
                            }
                        }
                        else
                        {
                            PrintMissignQuery(sql, rpc.Parameters);
                        }
                    }
                }
            }
            else
            {
                var text = rpc.ProcName.ToLowerInvariant();
                var result = TryGet(text, rpc.Parameters);
                if (null != result)
                {
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
                            new TDSDoneInProcToken(TDSDoneTokenStatusType.Count, TDSDoneTokenCommandType.DoneInProc,
                                result.Scalar);

                        return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response, doneIn));
                    }
                }
                else
                {
                    PrintMissignQuery(text, rpc.Parameters);
                }
            }
            return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response, done));
        }

        protected override TDSMessageCollection CreateQueryResponse(ITDSServerSession session, TDSSQLBatchToken batchRequest)
        {
            string lowerBatchText = batchRequest.Text.ToLowerInvariant().Trim();

            var result = TryGet(lowerBatchText, null);
            if (null != result)
            {
                List<TDSPacketToken> tokens = new List<TDSPacketToken>();
                TDSColMetadataToken metadataToken = new TDSColMetadataToken();
                tokens.Add(metadataToken);

                if (String.IsNullOrWhiteSpace(result.Table))
                {
                    TDSColumnData column = new TDSColumnData();
                    column.DataType = TDSDataType.IntN;
                    column.DataTypeSpecific = (byte)4;
                    column.Flags.Updatable = TDSColumnDataUpdatableFlag.ReadOnly;
                    column.Flags.IsComputed = true;
                    column.Flags.IsNullable = true;  // TODO: Must be nullable, otherwise something is wrong with SqlClient

                    // Add a column to the response
                    metadataToken.Columns.Add(column);

                    TDSRowToken rowToken = new TDSRowToken(metadataToken);
                    rowToken.Data.Add(result.Scalar);

                    tokens.Add(rowToken);

                    TDSDoneToken doneToken = new TDSDoneToken(TDSDoneTokenStatusType.Final | TDSDoneTokenStatusType.Count, TDSDoneTokenCommandType.Select, 1);
                    tokens.Add(doneToken);
                }
                else
                {
                    tokens.AddRange(ReturnTable(result));
                    // Create DONE token
                    TDSDoneToken doneToken = new TDSDoneToken(TDSDoneTokenStatusType.Final | TDSDoneTokenStatusType.Count,
                        TDSDoneTokenCommandType.Select, (ulong)result.GetRecords().Count());
                    tokens.Add(doneToken);
                }
                
                return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response, tokens.ToArray()));
            }
            else
            {
                PrintMissignQuery(lowerBatchText, null);
            }
            return base.CreateQueryResponse(session, batchRequest);
        }

        public void AddStub(SqlStub stub)
        {
            if (_stubs.ContainsKey(stub.Query))
            {
                _stubs[stub.Query].Add(stub);
            }
            else
            {
                var stubs = new List<SqlStub>();
                stubs.Add(stub);
                _stubs.Add(stub.Query, stubs);
            }
            
        }
    }
}
