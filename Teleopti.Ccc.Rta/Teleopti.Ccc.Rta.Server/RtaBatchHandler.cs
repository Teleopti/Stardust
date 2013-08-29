using System;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Caching;
using Teleopti.Ccc.Rta.Interfaces;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IRtaBatchHandler
	{
		IList<string> PeopleOnDataSource(int dataSourceId);
	}

	public class RtaBatchHandler : IRtaBatchHandler
	{
		private const string cacheKey = "BatchHandlerCache";
		private readonly ILog _log;
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly string _connectionString;
		private readonly Cache _cache;

		public RtaBatchHandler(IDatabaseConnectionFactory databaseConnectionFactory)
			: this(databaseConnectionFactory, LogManager.GetLogger(typeof(RtaBatchHandler)))
		{
		}

		private RtaBatchHandler(IDatabaseConnectionFactory databaseConnectionFactory, ILog log)
		{
			_databaseConnectionFactory = databaseConnectionFactory;			
	        _connectionString = ConfigurationManager.AppSettings["DataStore"];
			_log = log;
			_cache = HttpRuntime.Cache;
		}

		public IList<string> PeopleOnDataSource(int dataSourceId)
		{
			_log.InfoFormat("Getting people registered at datasource: {0}", dataSourceId);

			var dictionary = (IDictionary<int, List<string>>)_cache.Get(cacheKey) ?? buildDicitonary();
			if (dictionary.ContainsKey(dataSourceId))
			{
				var peopleOnDatSource = dictionary[dataSourceId];
				_log.InfoFormat("Found {0} persons on datasource: {1}", peopleOnDatSource.Count, dataSourceId);
				return peopleOnDatSource;
			}

			_log.InfoFormat("Could not find any people on datasource: {0}", dataSourceId);
			return null;
		}
		 
		private IDictionary<int, List<string>> buildDicitonary()
		{
			_log.Info("Loading new data into cache");

			var dataSourceLogOnMappings = new List<DataSourceLogOnMapping>();
			using (var connection = _databaseConnectionFactory.CreateConnection(_connectionString))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "RTA.rta_load_datasources_for_batch";
				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					dataSourceLogOnMappings.Add(new DataSourceLogOnMapping
						{
							DataSourceId = reader.GetInt16(reader.GetOrdinal("datasource_id")),
							ExternalLogOn = reader.GetString(reader.GetOrdinal("acd_login_original_id"))
						});
				}
				reader.Close();
			}

			_log.InfoFormat("Found {0} mappings", dataSourceLogOnMappings.Count);
			_log.Info("Done loading new data into cache");

			var dictionary = (dataSourceLogOnMappings
				.GroupBy(mapping => mapping.DataSourceId)
				.Select(mappedClass => mappedClass))
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Select(v => v.ExternalLogOn).ToList());

			_cache.Add(cacheKey, dictionary, null, DateTime.Now.AddMinutes(20), Cache.NoSlidingExpiration,
			           CacheItemPriority.Default, onRemoveCallback);
			return dictionary;
		}

		private void onRemoveCallback(string key, object value, CacheItemRemovedReason reason)
		{
			_log.Info("Cache was cleared");
			buildDicitonary();
		}

		private struct DataSourceLogOnMapping
		{
			public int DataSourceId { get; set; }
			public string ExternalLogOn { get; set; }
		}
	}
}
