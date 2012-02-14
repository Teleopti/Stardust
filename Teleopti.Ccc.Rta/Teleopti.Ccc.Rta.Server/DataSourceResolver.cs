using System;
using System.Collections.Generic;
using System.Data;
using log4net;
using Teleopti.Ccc.Rta.Interfaces;

namespace Teleopti.Ccc.Rta.Server
{
    public class DataSourceResolver : IDataSourceResolver
    {
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
        private readonly string _connectionStringDataStore;
        private readonly ILog _loggingSvc;
        private readonly IDictionary<string,int> _datasourceDictionary = new Dictionary<string, int>();
        private bool _isInitialized;

        public DataSourceResolver(IDatabaseConnectionFactory databaseConnectionFactory, string connectionStringDataStore) : this(databaseConnectionFactory,connectionStringDataStore,LogManager.GetLogger(typeof(DataSourceResolver)))
        {
        }

        protected DataSourceResolver(IDatabaseConnectionFactory databaseConnectionFactory, string connectionStringDataStore, ILog loggingSvc)
        {
            _databaseConnectionFactory = databaseConnectionFactory;
            _connectionStringDataStore = connectionStringDataStore;
            _loggingSvc = loggingSvc;
        }

        public bool TryResolveId(string sourceId, out int dataSourceId)
        {
            if (!_isInitialized)
            {
                using (var connection = _databaseConnectionFactory.CreateConnection(_connectionStringDataStore))
                {
                    var command = connection.CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "RTA.rta_load_datasources";
                    connection.Open();
                    var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        object loadedSourceId = reader["source_id"];
                        int loadedDataSourceId = reader.GetInt16(reader.GetOrdinal("datasource_id")); //This one cannot be null as it's the PK of the table
                        if (loadedSourceId == DBNull.Value)
                        {
                            _loggingSvc.WarnFormat("No source id is defined for data source = {0}",loadedDataSourceId);
                            continue;
                        }
                        var loadedSourceIdAsString = (string) loadedSourceId;
                        if (_datasourceDictionary.ContainsKey(loadedSourceIdAsString))
                        {
                            _loggingSvc.WarnFormat("There is already a source defined with the id = {0}",
                                                   loadedSourceIdAsString);
                            continue;
                        }
                        _datasourceDictionary.Add(loadedSourceIdAsString, loadedDataSourceId);
                    }
                    reader.Close();
                }
                _isInitialized = true;
            }

            return _datasourceDictionary.TryGetValue(sourceId, out dataSourceId);
        }
    }
}