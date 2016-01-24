using Autofac.Builder;
using Autofac.Extras.DynamicProxy2;
using Autofac.Features.Scanning;

namespace Teleopti.Ccc.Infrastructure.Aop
{
	public static class RegistrationExtensions
	{
		public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle>
			ApplyAspects<TLimit, TRegistrationStyle>(
			this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration)
		{
			return registration
				.EnableClassInterceptors()
				.ApplyFix()
				.InterceptedBy(typeof(AspectInterceptor))
				;
		}

		public static IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>
			ApplyAspects<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
			this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration)
			where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
		{
			return registration
				.EnableClassInterceptors()
				.ApplyFix()
				.InterceptedBy(typeof(AspectInterceptor))
				;
		}
	}
}