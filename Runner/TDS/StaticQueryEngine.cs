using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
    public class StaticQueryEngine : QueryEngine
    {
        public String Name { get; set; }

        private readonly Dictionary<String, List<SqlStub>> _exisitngStubs;
        private readonly List<SqlStub> _missingStubs;

        public StaticQueryEngine(TDSServerArguments arguments) : base(arguments)
        {
            _exisitngStubs = new Dictionary<string, List<SqlStub>>(StringComparer.OrdinalIgnoreCase);
            _missingStubs = new List<SqlStub>();
        }

        private void PrintMissignQuery(string sql, List<TDSRPCRequestParameter> parameters)
        {
            var newStub = new SqlStub();
            newStub.Query = sql;
            newStub.Parameters = new Dictionary<string, object>();

            lock (_exisitngStubs)
            {
                _missingStubs.Add(newStub);
                Console.WriteLine($"--------------{Name}---------------------");
                Console.WriteLine(sql);

                if (null != parameters)
                {
                    foreach (var parameter in parameters)
                    {
                        if (null != parameter.ParamMetaData && "@RETURN_VALUE" != parameter.ParamMetaData)
                        {
                            Console.WriteLine(parameter.ParamMetaData + " = " + parameter.Value);
                            newStub.Parameters.Add(parameter.ParamMetaData.Substring(1), parameter.Value);
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
                        column.DataTypeSpecific = new TDSShilohVarCharColumnSpecific((ushort) columnDefinition.Size,
                            new TDSColumnDataCollation(13632521, 52));
                    }
                    else if (columnDefinition.Type == TDSDataType.DateTime2N ||
                             columnDefinition.Type == TDSDataType.DateTime)
                    {
                        column.DataTypeSpecific = (byte) columnDefinition.Size;
                        column.Flags.IsComputed = true;
                        column.Flags.IsNullable =
                            true; // TODO: Must be nullable, otherwise something is wrong with SqlClient
                    }
                    else if (columnDefinition.Type == TDSDataType.Bit)
                    {
                    }
                    else if (columnDefinition.Type == TDSDataType.BigVarBinary)
                    {
                        column.DataTypeSpecific = (ushort) columnDefinition.Size;
                    }
                    else if (columnDefinition.Type == TDSDataType.Int2)
                    {
                        column.DataTypeSpecific = (byte) columnDefinition.Size;
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
            if (_exisitngStubs.ContainsKey(key))
            {
                var stubs = _exisitngStubs[key];
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
                            else if (result.ReturnValue.HasValue)
                            {
                                var retVal = new TDSReturnValueToken();
                                retVal.Flags.Updatable = TDSColumnDataUpdatableFlag.ReadOnly;
                                retVal.DataType = TDSDataType.IntN;
                                retVal.DataTypeSpecific = (byte) 4;
                                retVal.Flags.IsComputed = true;
                                retVal.Flags.IsNullable =
                                    true; // TODO: Must be nullable, otherwise something is wrong with SqlClient
                                retVal.ParamName = "@RETURN_VALUE";
                                retVal.Value = (int) result.ReturnValue.Value;
                                retVal.Status = TDSReturnValueStatus.Output;

                                TDSDoneInProcToken doneInRpc =
                                    new TDSDoneInProcToken(TDSDoneTokenStatusType.Count,
                                        TDSDoneTokenCommandType.DoneInProc, 1);
                                
                                return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response, retVal, doneInRpc));
                            }
                            else
                            {
                                TDSDoneInProcToken doneInRpc =
                                    new TDSDoneInProcToken(TDSDoneTokenStatusType.Count,
                                        TDSDoneTokenCommandType.DoneInProc,
                                        result.Scalar);

                                return new TDSMessageCollection(new TDSMessage(TDSMessageType.Response, doneInRpc));
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

        protected override TDSMessageCollection CreateQueryResponse(ITDSServerSession session,
            TDSSQLBatchToken batchRequest)
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
                    column.DataTypeSpecific = (byte) 4;
                    column.Flags.Updatable = TDSColumnDataUpdatableFlag.ReadOnly;
                    column.Flags.IsComputed = true;
                    column.Flags.IsNullable =
                        true; // TODO: Must be nullable, otherwise something is wrong with SqlClient

                    // Add a column to the response
                    metadataToken.Columns.Add(column);

                    TDSRowToken rowToken = new TDSRowToken(metadataToken);
                    rowToken.Data.Add(result.Scalar);

                    tokens.Add(rowToken);

                    TDSDoneToken doneToken =
                        new TDSDoneToken(TDSDoneTokenStatusType.Final | TDSDoneTokenStatusType.Count,
                            TDSDoneTokenCommandType.Select, 1);
                    tokens.Add(doneToken);
                }
                else
                {
                    tokens.AddRange(ReturnTable(result));
                    // Create DONE token
                    TDSDoneToken doneToken = new TDSDoneToken(
                        TDSDoneTokenStatusType.Final | TDSDoneTokenStatusType.Count,
                        TDSDoneTokenCommandType.Select, (ulong) result.GetRecords().Count());
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
            if (_exisitngStubs.ContainsKey(stub.Query))
            {
                _exisitngStubs[stub.Query].Add(stub);
            }
            else
            {
                var stubs = new List<SqlStub>();
                stubs.Add(stub);
                _exisitngStubs.Add(stub.Query, stubs);
            }

        }

        public IEnumerable<SqlStub> GetMissingStubs()
        {
            using (var hasher = MD5.Create())
                lock (_exisitngStubs)
                {
                    foreach (var stub in _missingStubs)
                    {
                        stub.Name = GetHash(stub, hasher);
                        if (stub.Parameters.Count == 0)
                        {
                            stub.Parameters = null;
                        }
                        yield return stub;
                    }
                }
        }

        private static String GetHash(SqlStub stub, MD5 hasher)
        {
            var parameters = String.Join(";", stub.Parameters.Select(p => p.Key + p.Value));
            var bytes = Encoding.UTF8.GetBytes(stub.Query + parameters);
            byte[] hash = hasher.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
