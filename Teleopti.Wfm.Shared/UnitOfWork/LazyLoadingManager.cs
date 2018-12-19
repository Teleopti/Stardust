using System.Collections.ObjectModel;
using System.Reflection;
using NHibernate;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    public static class LazyLoadingManager2
    {
        public static bool IsInitialized(object proxy)
        {
            return proxy == null || NHibernateUtil.IsInitialized(possibleUnderlyingProxy(proxy));
        }

        public static void Initialize(object proxy)
        {
            if(proxy!=null)
                NHibernateUtil.Initialize(possibleUnderlyingProxy(proxy));
        }

        private static object possibleUnderlyingProxy(object proxy)
        {
            var proxyType = proxy.GetType();
            if (proxyType.IsGenericType)
            {
                var genericUnderlyingType = proxyType.UnderlyingSystemType.GetGenericTypeDefinition();
                if(genericUnderlyingType.IsAssignableFrom(typeof(ReadOnlyCollection<>)))
                {
                    return proxyType.GetProperty("Items", BindingFlags.NonPublic | BindingFlags.Instance)
                                    .GetValue(proxy, null);   
                }
            }
            return proxy;
        }
    }
}
