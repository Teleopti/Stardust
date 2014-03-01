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
	}
}