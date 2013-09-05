using System;
using System.Collections.Concurrent;
using System.Data;
using System.Web;
using System.Web.Caching;
using Teleopti.Ccc.Rta.Interfaces;
using log4net;

namespace Teleopti.Ccc.Rta.Server.Resolvers
{
    public interface IStateResolver
    {
		bool HaveStateCodeChanged(Guid personId, string newStateCode, DateTime receivedTime);
	    void UpdateCacheForPerson(Guid personId, PersonStateHolder state);
    }

    public class StateResolver : IStateResolver
    {
        private const string cacheKey = "StateCache";
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

		public bool HaveStateCodeChanged(Guid personId, string newStateCode, DateTime receivedTime)
        {
			var dictionary = (ConcurrentDictionary<Guid, PersonStateHolder>)_cache.Get(cacheKey) ?? initialize();
			PersonStateHolder cachedStateHolder;
			var newStateHolder = new PersonStateHolder(newStateCode, receivedTime);

			if (!dictionary.TryGetValue(personId, out cachedStateHolder))
            {
				dictionary.TryAdd(personId, new PersonStateHolder(newStateCode, receivedTime));
                return true;
            }

			if (newStateHolder.StateCode != cachedStateHolder.StateCode &&
				newStateHolder.ReceivedTime > cachedStateHolder.ReceivedTime)
			{
				dictionary[personId] = newStateHolder;
				return true;
			}

        	return false;
        }

		private ConcurrentDictionary<Guid, PersonStateHolder> initialize()
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
					var timestamp = reader.GetDateTime(reader.GetOrdinal("ReceivedTime"));
	                var stateHolder = new PersonStateHolder(stateCode, timestamp);
					dictionary.AddOrUpdate(personId, stateHolder, (guid, oldState) => stateHolder.ReceivedTime > oldState.ReceivedTime ? stateHolder : oldState);
                }
                if (reader != null) reader.Close();
            }
            _cache.Add(cacheKey, dictionary, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration,
                       CacheItemPriority.Default, onRemoveCallback);
            _loggingSvc.Info("Done loading data into cache");
            return dictionary;
        }

		public void UpdateCacheForPerson(Guid personId, PersonStateHolder state)
		{
			((ConcurrentDictionary<Guid, PersonStateHolder>)_cache[cacheKey])[personId] = state;
		}

        private void onRemoveCallback(string key, object value, CacheItemRemovedReason reason)
        {
            _loggingSvc.Info("Statecache cleared");
            initialize();
        }
    }
}
