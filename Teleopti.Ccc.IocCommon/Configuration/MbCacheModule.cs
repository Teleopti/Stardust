using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using Autofac;
using MbCache.Configuration;
using MbCache.Core;
using MbCache.Core.Events;
using MbCache.ProxyImpl.LinFu;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class MbCacheModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public MbCacheModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}
		
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CachePerDataSource>().SingleInstance();
			builder.RegisterType<TeleoptiCacheKey>().SingleInstance();
			builder.RegisterType<LinFuProxyFactory>().SingleInstance();
			if (_configuration.Toggle(Toggles.ResourcePlanner_XXL_47258))
			{
				builder.Register(c =>
				{
					var cacheBuilder = new CacheBuilder(c.Resolve<LinFuProxyFactory>())
						.SetCacheKey(c.Resolve<TeleoptiCacheKey>())
						.SetCache(new InMemoryCache2(new FixedNumberOfLockObjects(100), 20))
						.AddEventListener(new MbCacheLog4NetListener());
					_configuration.Cache().Build(c, cacheBuilder);
					return cacheBuilder;
				}).SingleInstance();
			}
			else
			{
				builder.Register(c =>
				{
					var cacheBuilder = new CacheBuilder(c.Resolve<LinFuProxyFactory>())
						.SetCacheKey(c.Resolve<TeleoptiCacheKey>())
						.AddEventListener(new MbCacheLog4NetListener());
					_configuration.Cache().Build(c, cacheBuilder);
					return cacheBuilder;
				}).SingleInstance();				
			}
			builder.Register(c => c.Resolve<CacheBuilder>().BuildFactory())
				.As<IMbCacheFactory>()
				.SingleInstance();
		}
	}
	
	
	[RemoveMeWithToggle("Just a hack for now", Toggles.ResourcePlanner_XXL_47258)]
	[Serializable]
	public class InMemoryCache2 : ICache
	{
		private readonly ILockObjectGenerator _lockObjectGenerator;
		private readonly int _timeoutMinutes;
		private static readonly MemoryCache cache = MemoryCache.Default;
		private static readonly object dependencyValue = new object();
		private EventListenersCallback _eventListenersCallback;
		private const string mainCacheKey = "MainMbCacheKey";
		private readonly ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		public InMemoryCache2(ILockObjectGenerator lockObjectGenerator, int timeoutMinutes)
		{
			_lockObjectGenerator = lockObjectGenerator;
			_timeoutMinutes = timeoutMinutes;
		}

		public void Initialize(EventListenersCallback eventListenersCallback)
		{
			_eventListenersCallback = eventListenersCallback;
		}

		public CachedItem GetAndPutIfNonExisting(EventInformation eventInformation, Func<IEnumerable<string>> dependingRemoveKeys, Func<object> originalMethod)
		{
			var cachedItem = (CachedItem)cache.Get(eventInformation.CacheKey);
			if (cachedItem != null)
			{
				_eventListenersCallback.OnCacheHit(cachedItem);
				return cachedItem;
			}

			lock (lockObject(eventInformation))
			{
				var cachedItem2 = (CachedItem)cache.Get(eventInformation.CacheKey);
				if (cachedItem2 != null)
				{
					_eventListenersCallback.OnCacheHit(cachedItem2);
					return cachedItem2;
				}
				var addedValue = executeAndPutInCache(eventInformation, dependingRemoveKeys(), originalMethod);
				_eventListenersCallback.OnCacheMiss(addedValue);
				return addedValue;
			}
		}

		public void Delete(EventInformation eventInformation)
		{
			_readerWriterLockSlim.EnterWriteLock();
			try
			{
				cache.Remove(eventInformation.CacheKey);
			}
			finally
			{
				_readerWriterLockSlim.ExitWriteLock();
			}
		}

		public void Clear()
		{
			_readerWriterLockSlim.EnterWriteLock();
			try
			{
				cache.Remove(mainCacheKey);
			}
			finally
			{
				_readerWriterLockSlim.ExitWriteLock();
			}
		}

		private object lockObject(EventInformation eventInformation)
		{
			return _lockObjectGenerator.GetFor(eventInformation.CacheKey);
		}

		private CachedItem executeAndPutInCache(EventInformation eventInformation, IEnumerable<string> dependingRemoveKeys, Func<object> originalMethod)
		{
			//var poo = _readerWriterLockSlim.IsReadLockHeld;
			//if(!poo)
				_readerWriterLockSlim.EnterReadLock();
			try
			{
				var methodResult = originalMethod();
				var cachedItem = new CachedItem(eventInformation, methodResult);
				var key = cachedItem.EventInformation.CacheKey;
				var dependedKeys = dependingRemoveKeys.ToList();
				dependedKeys.Add(mainCacheKey);
				createDependencies(dependedKeys);

				var policy = new CacheItemPolicy
				{
					AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(_timeoutMinutes),
					RemovedCallback = arguments => _eventListenersCallback.OnCacheRemoval(cachedItem)
				};
				policy.ChangeMonitors.Add(cache.CreateCacheEntryChangeMonitor(dependedKeys));
				cache.Set(key, cachedItem, policy);
				return cachedItem;
			}
			finally
			{
				//if(poo)
					_readerWriterLockSlim.ExitReadLock();
			}
		}

		private static void createDependencies(IEnumerable<string> unwrappedKeys)
		{
			foreach (var key in unwrappedKeys)
			{
				var policy = new CacheItemPolicy { Priority = CacheItemPriority.NotRemovable };
				cache.Add(key, dependencyValue, policy);
			}
		}
	}
}