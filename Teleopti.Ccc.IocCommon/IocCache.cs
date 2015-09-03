using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Builder;
using MbCache.Configuration;
using MbCache.Core;

namespace Teleopti.Ccc.IocCommon
{
	public class IocCache
	{
		private class cacheRegistration
		{
			public Action<CacheBuilder> BuilderAction;
		}

		private readonly List<cacheRegistration> _cacheRegistrations = new List<cacheRegistration>();

		public void This<T>(Func<IFluentBuilder<T>, IFluentBuilder<T>> cachedMethods) where T : class
		{
			_cacheRegistrations.Add(new cacheRegistration
			{
				BuilderAction = b =>
				{
					cachedMethods.Invoke(b.For<T>()).As<T>();
				}
			});
		}
		
		public void Build(CacheBuilder builder)
		{
			_cacheRegistrations.ForEach(c =>
			{
				c.BuilderAction.Invoke(builder);
			});
		}
		
	}

	public static class AutofacMbCacheIntegration
	{
		public static IRegistrationBuilder<TInterface, SimpleActivatorData, SingleRegistrationStyle> CacheByInterfaceProxy<TClass, TInterface>(this ContainerBuilder containerBuilder)
			where TInterface : class
			where TClass : TInterface
		{
			containerBuilder
				.RegisterType<TClass>()
				.AsSelf();
			return containerBuilder
				.Register<TInterface>(c => c.Resolve<TClass>())
				.OnActivating(e =>
				{
					e.ReplaceInstance(e.Context.Resolve<IMbCacheFactory>().ToCachedComponent(e.Instance));
				});
		}

		public static IRegistrationBuilder<TClass, ConcreteReflectionActivatorData, SingleRegistrationStyle> CacheByClassProxy<TClass>(this ContainerBuilder containerBuilder)
			where TClass : class
		{
			return containerBuilder
				.RegisterType<TClass>()
				.OnActivating(e => e.ReplaceInstance(e.Context.Resolve<IMbCacheFactory>().ToCachedComponent(e.Instance)));
		}
	}


}