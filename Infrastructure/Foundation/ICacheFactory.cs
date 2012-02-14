using MbCache.Core;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    public interface ICacheFactory
    {
        IMbCacheFactory MbCacheFactory { get; }
    }
}