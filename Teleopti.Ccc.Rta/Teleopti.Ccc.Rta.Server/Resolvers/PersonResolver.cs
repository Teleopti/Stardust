using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Caching;
using log4net;

namespace Teleopti.Ccc.Rta.Server.Resolvers
{
	public class PersonResolver : IPersonResolver
	{
		private const string cacheKey = "PeopleCache";
		private readonly Cache _cache;
		private readonly IDatabaseReader _databaseReader;

		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(IPersonResolver));
		
		public PersonResolver(IDatabaseReader databaseReader)
		{
			_databaseReader = databaseReader;
			_cache = HttpRuntime.Cache;
		}

		public bool TryResolveId(int dataSourceId, string logOn, out IEnumerable<PersonWithBusinessUnit> personId)
		{
			var lookupKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", dataSourceId, logOn).ToUpper(CultureInfo.InvariantCulture);
			if (string.IsNullOrEmpty(logOn))
				lookupKey = string.Empty;

			var dictionary = (ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>>)_cache.Get(cacheKey) ??
							 initialize();

			return dictionary.TryGetValue(lookupKey, out personId);
		}

		private ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>> initialize()
		{
			LoggingSvc.Debug("Loading new data into cache.");
			
			var dictionary = _databaseReader.LoadAllExternalLogOns();
			addBatchSignature(dictionary);
			_cache.Add(cacheKey, dictionary, null, DateTime.Now.AddMinutes(10), Cache.NoSlidingExpiration,
					   CacheItemPriority.Default, onRemoveCallback);

			LoggingSvc.Debug("Done loading new data into cache.");

			return dictionary;
		}

		private static void addBatchSignature(ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>> dictionary)
		{
			IEnumerable<PersonWithBusinessUnit> foundIds;
			if (dictionary.TryGetValue(string.Empty, out foundIds)) return;
			var emptyArray = new[] { new PersonWithBusinessUnit { PersonId = Guid.Empty, BusinessUnitId = Guid.Empty } };
			dictionary.AddOrUpdate(string.Empty, emptyArray, (s, units) => emptyArray);
		}

		private void onRemoveCallback(string key, object value, CacheItemRemovedReason reason)
		{
			LoggingSvc.Debug("The cache was cleared.");
			initialize();
		}
	}

	public struct PersonWithBusinessUnit
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}
}