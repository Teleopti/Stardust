using System;
using System.Web;
using System.Web.Caching;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation.Cache
{
    //extremely simple for now.
    //maybe..
    //* add regions (to invalidate in group)
    //* some call backs
    //* invalidate by passing some object knowing about mb?
    public class AspNetCache<T> : ICustomDataCache<T>
    {
        private readonly System.Web.Caching.Cache cache;
        private readonly string keyPrefix;

        public AspNetCache()
        {
            TimeoutMinutes = 20;
            cache = HttpRuntime.Cache;
            keyPrefix = typeof (T).FullName + ":";
        }

        public int TimeoutMinutes { get; set; }

        public void Delete(string key)
        {
            cache.Remove(realKey(key));
        }

        public T Get(string key)
        {
            return (T)cache.Get(realKey(key));
        }

        public void Put(string key, T value)
        {
            cache.Insert(realKey(key), 
                    value, 
                    null, 
                    DateTime.Now.Add(TimeSpan.FromMinutes(TimeoutMinutes)), 
                    System.Web.Caching.Cache.NoSlidingExpiration, 
                    CacheItemPriority.Default, 
                    null);
        }

        private string realKey(string key)
        {
            return string.Concat(keyPrefix + key);
        }
    }
}


