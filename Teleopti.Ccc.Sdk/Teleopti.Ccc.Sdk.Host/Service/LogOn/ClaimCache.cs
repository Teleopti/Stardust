using System;
using System.IdentityModel.Claims;
using System.Web;
using System.Web.Caching;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
    public class ClaimCache
    {
        private readonly Cache _cache = HttpRuntime.Cache;

        public void Add(ClaimSet claimSet, Guid roleId)
        {
            string key = roleId.ToString();
            _cache.Add(key, claimSet, null, DateTime.Now.AddMinutes(30), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public ClaimSet Get(Guid roleId)
        {
            string key = roleId.ToString();
            return _cache[key] as ClaimSet;
        }
    }
}