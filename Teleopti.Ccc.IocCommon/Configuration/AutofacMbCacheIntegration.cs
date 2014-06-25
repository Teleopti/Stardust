using Autofac;
using Autofac.Builder;
using MbCache.Core;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public static class AutofacMbCacheIntegration
	{
		public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> IntegrateWithMbCache<T>(this IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> builder) where T : class
		{
			return builder.OnActivating(e => e.ReplaceInstance(e.Context.Resolve<IMbCacheFactory>().ToCachedComponent(e.Instance)));
		}

		public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> IntegrateWithMbCache<T>(this IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> builder) where T : class
		{
			return builder.OnActivating(e => e.ReplaceInstance(e.Context.Resolve<IMbCacheFactory>().ToCachedComponent(e.Instance)));
		}


		public static IRegistrationBuilder<TInterface, SimpleActivatorData, SingleRegistrationStyle> RegisterMbCacheComponent<TClass, TInterface>(this ContainerBuilder containerBuilder)
			where TInterface : class
			where TClass : TInterface
		{
			containerBuilder.RegisterType<TClass>().AsSelf();
			return containerBuilder.Register<TInterface>(c => c.Resolve<TClass>())
				.OnActivating(e => e.ReplaceInstance(e.Context.Resolve<IMbCacheFactory>().ToCachedComponent(e.Instance)));
		}
	}
}