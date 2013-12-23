using System;
using System.Collections.Concurrent;
using System.Web;
using System.Web.Caching;
using log4net;

namespace Teleopti.Ccc.Rta.Server.Resolvers
{
	public class DataSourceResolver : IDataSourceResolver
	{
		private const string cacheKey = "DataSourceCache";
		private readonly Cache _cache;
		private readonly IDatabaseReader _databaseReader;

		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(IDataSourceResolver));

		public DataSourceResolver(IDatabaseReader databaseReader)
		{
			_databaseReader = databaseReader;
			_cache = HttpRuntime.Cache;
		}

		public bool TryResolveId(string sourceId, out int dataSourceId)
		{
			var dictionary = (ConcurrentDictionary<string, int>)_cache.Get(cacheKey) ?? initialize();
			return dictionary.TryGetValue(sourceId, out dataSourceId);
		}

		private ConcurrentDictionary<string, int> initialize()
		{
			LoggingSvc.Debug("Loading new data into cache.");

			var dictionary = _databaseReader.LoadDatasources();

			_cache.Add(cacheKey, dictionary, null, DateTime.Now.AddMinutes(10), Cache.NoSlidingExpiration,
					   CacheItemPriority.Default, onRemoveCallback);

			LoggingSvc.Debug("Done loading new data into cache.");

			return dictionary;
		}

		private void onRemoveCallback(string key, object value, CacheItemRemovedReason reason)
		{
			LoggingSvc.Debug("The cache was cleared.");
			initialize();

		}
	}
}