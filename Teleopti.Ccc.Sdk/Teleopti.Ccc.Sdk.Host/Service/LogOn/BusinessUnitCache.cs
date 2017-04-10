using System;
using System.Web;
using System.Web.Caching;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
    public class BusinessUnitCache
    {
        private readonly Cache _cache = HttpRuntime.Cache;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void Add(IBusinessUnit businessUnit)
        {
            string key = businessUnit.Id.ToString();
            _cache.Add(key, businessUnit, null, DateTime.Now.AddMinutes(30), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public IBusinessUnit Get(Guid key)
        {
            return _cache[key.ToString()] as IBusinessUnit;
        }
    }
}