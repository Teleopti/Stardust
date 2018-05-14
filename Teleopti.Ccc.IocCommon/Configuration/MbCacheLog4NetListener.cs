using log4net;
using MbCache.Core;
using MbCache.Core.Events;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class MbCacheLog4NetListener : IEventListener
	{
		private const string putMessage = "Adding cache value {0} for {1}";
		private const string deleteMessage = "Removing cache entries for {0}";
		private const string cacheHitLogMessage = "Cache hit for {0}";
		private const string nullReplacer = "[all methods]";

		private static readonly ILog log = LogManager.GetLogger("Teleopti.MbCache");

		public void OnCacheHit(CachedItem cachedItem)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat(cacheHitLogMessage, cachedItem.CachedMethodInformation.Method?.ToString() ?? nullReplacer);
			}
		}

		public void OnCacheRemoval(CachedItem cachedItem)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat(deleteMessage, cachedItem.CachedMethodInformation.Method?.ToString() ?? nullReplacer);
			}
		}

		public void OnCacheMiss(CachedItem cachedItem)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat(putMessage, cachedItem.CachedValue, cachedItem.CachedMethodInformation.Method?.ToString() ?? nullReplacer);
			}
		}

		public void Warning(string warnMessage)
		{
			log.Warn(warnMessage);
		}
	}
}