using Autofac;
using Autofac.Builder;
using MbCache.Core;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public static class AutofacMbCacheIntegration
	{
		public static IRegistrationBuilder<TInterface, SimpleActivatorData, SingleRegistrationStyle> RegisterMbCacheComponent<TClass, TInterface>(this ContainerBuilder containerBuilder)
			where TInterface : class
			where TClass : TInterface
		{
			containerBuilder.RegisterType<TClass>().AsSelf();
			return containerBuilder.Register<TInterface>(c => c.Resolve<TClass>())
				.OnActivating(e => e.ReplaceInstance(e.Context.Resolve<IMbCacheFactory>().ToCachedComponent(e.Instance)));
		}

		public static IRegistrationBuilder<TClass, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterConcreteMbCacheComponent<TClass>(this ContainerBuilder containerBuilder)
			where TClass : class
		{
			return containerBuilder.RegisterType<TClass>()
				.OnActivating(e => e.ReplaceInstance(e.Context.Resolve<IMbCacheFactory>().ToCachedComponent(e.Instance)));
		}
	}
}