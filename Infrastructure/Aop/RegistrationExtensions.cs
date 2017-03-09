using System;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Extras.DynamicProxy2;
using Autofac.Features.Scanning;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.Aop
{
	public static class RegistrationExtensions
	{
		public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle>
			ApplyAspects<TLimit, TRegistrationStyle>(
			this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration)
		{
			return registration
				.OnActivating(e => validateAspectedType(ProxyUtil.GetUnproxiedType(e.Instance)))
				.EnableClassInterceptors()
				.ApplyFix()
				.InterceptedBy(typeof (AspectInterceptor))
				;
		}

		public static IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>
			ApplyAspects<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
			this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration)
			where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
		{
			validateAspectedType(registration.ActivatorData.ImplementationType);
			return registration
				.EnableClassInterceptors()
				.ApplyFix()
				.InterceptedBy(typeof(AspectInterceptor))
				;
		}

		private static void validateAspectedType(Type type)
		{
			var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			var invalidMethods =
				from m in methods
				let hasAspect = m.GetCustomAttributes<AspectAttribute>().Any()
				let isVirtual = m.IsVirtual
				where hasAspect && !isVirtual
				select m;

			var invalidSample = invalidMethods.FirstOrDefault();
			if (invalidSample != null)
				throw new AspectApplicationException($"Aspected methods needs to be virtual. {invalidSample.Name} is not.");
		}

	}
}