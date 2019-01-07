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
		private readonly List<Action<IComponentContext, CacheBuilder>> _builderActions = new List<Action<IComponentContext, CacheBuilder>>();

		public void This<T>(Func<FluentBuilder<T>, FluentBuilder<T>> cachedMethods) where T : class
		{
			_builderActions.Add((c, b) =>
			{
				cachedMethods.Invoke(b.For<T>()).As<T>();
			});
		}

		public void This<T>(Func<FluentBuilder<T>, FluentBuilder<T>> cachedMethods, string cacheKeyForType) where T : class
		{
			_builderActions.Add((c, b) =>
			{
				cachedMethods.Invoke(b.For<T>(cacheKeyForType)).As<T>();
			});
		}

		public void This<T>(Func<IComponentContext, FluentBuilder<T>, FluentBuilder<T>> cachedMethods) where T : class
		{
			_builderActions.Add((c, b) =>
			{
				cachedMethods.Invoke(c, b.For<T>()).As<T>();
			});
		}

		public void Build(IComponentContext componentContext, CacheBuilder builder)
		{
			_builderActions.ForEach(a =>
			{
				a.Invoke(componentContext, builder);
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