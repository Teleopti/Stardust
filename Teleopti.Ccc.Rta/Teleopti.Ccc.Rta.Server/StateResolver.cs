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
        bool HaveStateCodeChanged(Guid personId, string newStateCode, DateTime timestamp);
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
        
        public bool HaveStateCodeChanged(Guid personId, string newStateCode, DateTime timestamp)
        {
			var dictionary = (ConcurrentDictionary<Guid, PersonStateHolder>)_cache.Get(CacheKey) ?? Initialize();
			PersonStateHolder cachedStateHolder;
	        var newStateHolder = new PersonStateHolder(newStateCode, timestamp);

			if (!dictionary.TryGetValue(personId, out cachedStateHolder))
            {
                dictionary.TryAdd(personId, new PersonStateHolder(newStateCode, timestamp));
                return true;
            }

			if (newStateHolder.StateCode != cachedStateHolder.StateCode && 
				newStateHolder.Timestamp > cachedStateHolder.Timestamp)
			{
				dictionary[personId] = newStateHolder;
				return true;
			}

        	return false;
        }

		private ConcurrentDictionary<Guid, PersonStateHolder> Initialize()
        {
            _loggingSvc.Info("Loading new data into state cache");
			var dictionary = new ConcurrentDictionary<Guid, PersonStateHolder>();
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
	                var timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"));
	                var stateHolder = new PersonStateHolder(stateCode, timestamp);
	                dictionary.AddOrUpdate(personId, stateHolder, (guid, oldState) => stateHolder.Timestamp > oldState.Timestamp ? stateHolder : oldState);
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

		public class PersonStateHolder
		{
			public string StateCode { get; set; }
			public DateTime Timestamp { get; set; }

			public PersonStateHolder(string stateCode, DateTime timestamp)
			{
				StateCode = stateCode;
				Timestamp = timestamp;
			}

			public override bool Equals(object obj)
			{
				return obj != null && GetHashCode().Equals(obj.GetHashCode());
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var result = 0;
					result = (result*397) ^ StateCode.GetHashCode();
					result = (result*397) ^ Timestamp.GetHashCode();
					return result;
				}
			}
		}
    }
}
