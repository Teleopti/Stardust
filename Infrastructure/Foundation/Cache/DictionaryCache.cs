using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation.Cache
{
    //note: not thread safe för fem öre!
    public class DictionaryCache<T> : ICustomDataCache<T>
    {
        private readonly static IDictionary<string, T> _cache = new Dictionary<string, T>();
        private readonly string keyPrefix;

        public DictionaryCache()
        {
            keyPrefix = typeof (T).FullName + ":";
        }

        public T Get(string key)
        {
            T ret;
            _cache.TryGetValue(realKey(key), out ret);
            return ret;
        }

        public void Put(string key, T value)
        {
            _cache[realKey(key)] = value;
        }

        public void Delete(string key)
        {
            _cache.Remove(realKey(key));
        }

        //good to have when using this in test. not part of the interface
        public void Clear()
        {
            _cache.Clear();
        }

        private string realKey(string key)
        {
            return string.Concat(keyPrefix + key);
        }
    }
}
