using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation.Cache
{
    public class NoCache<T> : ICustomDataCache<T>
    {
        public T Get(string key)
        {
            return default(T);
        }

        public void Put(string key, T value)
        {
        }

        public void Delete(string key)
        {
        }

        public void Clear()
        {
        }

        public int Count()
        {
            return 0;
        }
    }
}
