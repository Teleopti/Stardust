using System.Collections.ObjectModel;
using System.Reflection;
using NHibernate;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Infrastructure.Foundation
{

    public interface ILazyLoadingManager 
    {
        bool IsInitialized(object proxy);
        void Initialize(object proxy);
    }

    public class LazyLoadingManagerWrapper : ILazyLoadingManager 
    {
        public bool IsInitialized(object proxy) 
        {
            return LazyLoadingManager.IsInitialized(proxy);
        }

        public void Initialize(object proxy) 
        {
            LazyLoadingManager.Initialize(proxy);
        }
    }

    /// <summary>
    /// A manager for lazy loading
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-12-05
    /// </remarks>
    public static class LazyLoadingManager
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

        //todo: rk -snygga till
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
                if(genericUnderlyingType.Equals(typeof(LayerCollection<>)))
                {
                    return proxyType.GetProperty("Items", BindingFlags.NonPublic | BindingFlags.Instance)
                                    .GetValue(proxy, null);  
                }
            }
            return proxy;
        }
    }
}
