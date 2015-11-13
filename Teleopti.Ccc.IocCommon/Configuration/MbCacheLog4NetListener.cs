using log4net;
using MbCache.Core;
using MbCache.Core.Events;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class MbCacheLog4NetListener : IEventListener
	{
		private const string putMessage = "Adding cache value {0} for {1} (key: {2})";
		private const string deleteMessage = "Removing cache entries for {0} (key starts with: {1})";
		private const string cacheHitLogMessage = "Cache hit for {0} (key: {1})";

		private static readonly ILog log = LogManager.GetLogger("Teleopti.MbCache");

		public void OnCacheHit(CachedItem cachedItem)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat(cacheHitLogMessage, cachedItem.EventInformation.MethodName(), cachedItem.EventInformation.CacheKey);
			}
		}

		public void OnCacheRemoval(CachedItem cachedItem)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat(deleteMessage, cachedItem.EventInformation.MethodName(), cachedItem.EventInformation.CacheKey);
			}
		}

		public void OnCacheMiss(CachedItem cachedItem)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat(putMessage, cachedItem.CachedValue, cachedItem.EventInformation.MethodName(), cachedItem.EventInformation.CacheKey);
			}
		}

		public void Warning(string warnMessage)
		{
			log.Warn(warnMessage);
		}
	}
}