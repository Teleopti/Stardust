using MbCache.Core;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    public class CacheFactory : ICacheFactory
    {
        public CacheFactory(IMbCacheFactory mbCacheFactory)
        {
            MbCacheFactory = mbCacheFactory;
        }

        public IMbCacheFactory MbCacheFactory { get; private set; }
    }
}
