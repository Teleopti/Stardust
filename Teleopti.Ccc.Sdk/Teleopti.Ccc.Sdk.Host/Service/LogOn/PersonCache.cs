using System;
using System.Web;
using System.Web.Caching;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
    public class PersonCache
    {
        private readonly Cache _cache = HttpRuntime.Cache;
        
        public void Add(PersonContainer person)
        {
            string key = string.Concat(person.DataSource, "|", person.UserName);
            if (!string.IsNullOrEmpty(person.Password))
            {
                key = string.Concat(key, "|", person.Password);
            }
            Add(person,key);
        }

        public void Add(PersonContainer person, string key)
        {
            _cache.Add(key, person, null, DateTime.Now.AddMinutes(30), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public PersonContainer Get(string dataSource, string userName, string password)
        {
            string key = string.Concat(dataSource, "|", userName, "|", password);
            return Get(key);
        }

        public PersonContainer Get(string dataSource, string windowsUser)
        {
            string key = string.Concat(dataSource, "|", windowsUser);
            return Get(key);
        }

        public PersonContainer Get(string key)
        {
            return _cache[key] as PersonContainer;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Remove(PersonContainer personContainer)
        {
            if (IsWindowsUser(personContainer))
            {
                _cache.Remove(string.Concat(personContainer.DataSource, "|", personContainer.UserName));
            }
            else
            {
                _cache.Remove(string.Concat(personContainer.DataSource, "|", personContainer.UserName, "|",
                                        personContainer.Password));
            }
        }

        private static bool IsWindowsUser(PersonContainer personContainer)
        {
            return string.IsNullOrEmpty(personContainer.Password);
        }
    }
}