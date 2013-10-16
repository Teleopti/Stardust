using System;
using System.Collections.Concurrent;
using System.Data;
using System.Web;
using System.Web.Caching;
using log4net;
using Teleopti.Ccc.Rta.Interfaces;

namespace Teleopti.Ccc.Rta.Server.Resolvers
{
	public interface IDataSourceResolver
	{
		bool TryResolveId(string sourceId, out int dataSourceId);
	}

	public class DataSourceResolver : IDataSourceResolver
	{
		private const string CacheKey = "DataSourceCache";
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly string _connectionStringDataStore;
		private readonly ILog _loggingSvc;
		private readonly Cache _cache;

		public DataSourceResolver(IDatabaseConnectionFactory databaseConnectionFactory, string connectionStringDataStore)
			: this(databaseConnectionFactory, connectionStringDataStore, LogManager.GetLogger(typeof(DataSourceResolver)))
		{
		}

		protected DataSourceResolver(IDatabaseConnectionFactory databaseConnectionFactory, string connectionStringDataStore, ILog loggingSvc)
		{
			_databaseConnectionFactory = databaseConnectionFactory;
			_connectionStringDataStore = connectionStringDataStore;
			_loggingSvc = loggingSvc;

			_cache = HttpRuntime.Cache;
		}

		public bool TryResolveId(string sourceId, out int dataSourceId)
		{
			var dictionary = (ConcurrentDictionary<string, int>)_cache.Get(CacheKey) ?? initialize();
			return dictionary.TryGetValue(sourceId, out dataSourceId);
		}

		private ConcurrentDictionary<string, int> initialize()
		{
			_loggingSvc.Debug("Loading new data into cache.");

			var dictionary = new ConcurrentDictionary<string, int>();
			using (var connection = _databaseConnectionFactory.CreateConnection(_connectionStringDataStore))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "RTA.rta_load_datasources";
				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					var loadedSourceId = reader["source_id"];
					int loadedDataSourceId = reader.GetInt16(reader.GetOrdinal("datasource_id")); //This one cannot be null as it's the PK of the table
					if (loadedSourceId == DBNull.Value)
					{
						_loggingSvc.WarnFormat("No source id is defined for data source = {0}", loadedDataSourceId);
						continue;
					}
					var loadedSourceIdAsString = (string)loadedSourceId;
					if (dictionary.ContainsKey(loadedSourceIdAsString))
					{
						_loggingSvc.WarnFormat("There is already a source defined with the id = {0}",
											   loadedSourceIdAsString);
						continue;
					}
					dictionary.AddOrUpdate(loadedSourceIdAsString, loadedDataSourceId, (s, i) => loadedDataSourceId);
				}
				reader.Close();
			}

			_cache.Add(CacheKey, dictionary, null, DateTime.Now.AddMinutes(10), Cache.NoSlidingExpiration,
					   CacheItemPriority.Default, onRemoveCallback);

			_loggingSvc.Debug("Done loading new data into cache.");

			return dictionary;
		}

		private void onRemoveCallback(string key, object value, CacheItemRemovedReason reason)
		{
			_loggingSvc.Debug("The cache was cleared.");
			initialize();

		}
	}
}