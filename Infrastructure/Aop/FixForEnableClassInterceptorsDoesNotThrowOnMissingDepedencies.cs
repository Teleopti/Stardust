using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Features.Scanning;

namespace Teleopti.Ccc.Infrastructure.Aop
{
	public static class FixForEnableClassInterceptorsDoesNotThrowOnMissingDepedencies
	{
		public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle>
			ApplyFix<TLimit, TRegistrationStyle>(
			this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration)
		{
			return registration.OnPreparing(fix);
		}

		public static IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>
			ApplyFix<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
			this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration)
			where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
		{
			return registration.OnPreparing(fix);
		}

		private static void fix(PreparingEventArgs e)
		{
			var activator = e.Component.Activator;
			var activatorType = activator.GetType();
			var defaultParametersField = activatorType.GetField("_defaultParameters",
				BindingFlags.Instance | BindingFlags.NonPublic);
			var defaultParameters = defaultParametersField.GetValue(activator) as IEnumerable<Parameter>;

			var fixedDefaultParameters = (
				from p in defaultParameters
				let replaced = p.GetType() == typeof (DefaultValueParameter) ? new FixedDefaultValueParameter() : p
				select replaced
				).ToArray();

			defaultParametersField.SetValue(activator, fixedDefaultParameters);
		}

		private class FixedDefaultValueParameter : Parameter
		{
			public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object> valueProvider)
			{
				if (pi == null)
				{
					throw new ArgumentNullException(nameof(pi));
				}

				if (pi.IsOptional)
				{
					valueProvider = () => pi.DefaultValue;
					return true;
				}
				valueProvider = null;
				return false;
			}

		}
	}
}