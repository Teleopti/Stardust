using System;
using System.Web;
using System.Web.Caching;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
	public class PersonInRoleCache
	{
		private readonly Cache _cache = HttpRuntime.Cache;

		public void Add(PersonInRoleCacheItem cacheItem, Guid personId)
		{
			string key = personId.ToString();
			_cache.Add(key, cacheItem, null, DateTime.Now.AddMinutes(30), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
		}

		public PersonInRoleCacheItem Get(Guid personId)
		{
			string key = personId.ToString();
			return _cache[key] as PersonInRoleCacheItem;
		}
	}
}