using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Caching;
using Teleopti.Ccc.Rta.Interfaces;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
    public interface IStateResolver
    {
        bool HaveStateCodeChanged(Guid personId, string newStateCode);
    }

    public class StateResolver : IStateResolver
    {
        private const string CacheKey = "StateCache";
        private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
        private readonly string _connectionStringDataStore;
        private readonly ILog _loggingSvc;
        private readonly Cache _cache;

        public StateResolver(IDatabaseConnectionFactory databaseConnectionFactory, string connectionStringDataStore)
            : this(databaseConnectionFactory, connectionStringDataStore, LogManager.GetLogger(typeof (StateResolver)))
        {
        }

        protected StateResolver(IDatabaseConnectionFactory databaseConnectionFactory, string connectionStringDataStore, ILog loggingSvc)
        {
            _databaseConnectionFactory = databaseConnectionFactory;
            _connectionStringDataStore = connectionStringDataStore;
            _loggingSvc = loggingSvc;
            _cache = HttpRuntime.Cache;
        }
        
        public bool HaveStateCodeChanged(Guid personId, string newStateCode)
        {
            var dictionary = (ConcurrentDictionary<Guid, string>) _cache.Get(CacheKey) ?? Initialize();
            string result;
			
			if (!dictionary.TryGetValue(personId, out result))
            {
                dictionary.Add(personId, newStateCode);
                return true;
            }

			if (result != newStateCode)
			{
				dictionary[personId] = newStateCode;
				return true;
			}

        	return false;
        }

        private IDictionary<Guid, string> Initialize()
        {
            _loggingSvc.Info("Loading new data into state cache");
            var dictionary = new ConcurrentDictionary<Guid, string>();
            using (var connection = _databaseConnectionFactory.CreateConnection(_connectionStringDataStore))
            {
                var command = connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "RTA.rta_load_actual_agent_statecode";
                connection.Open();
                var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                while (reader != null && reader.Read())
                {
                    var stateCode = reader.GetString(reader.GetOrdinal("StateCode"));
                    var personId = reader.GetGuid(reader.GetOrdinal("PersonId"));
                    dictionary.AddOrUpdate(personId, stateCode, (guid, s) => s);
                }
                if (reader != null) reader.Close();
            }
            _cache.Add(CacheKey, dictionary, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration,
                       CacheItemPriority.Default, OnRemoveCallback);
            _loggingSvc.Info("Done loading data into cache");
            return dictionary;
        }

        private void OnRemoveCallback(string key, object value, CacheItemRemovedReason reason)
        {
            _loggingSvc.Info("Statecache cleared");
            Initialize();
        }
    }
}
