using System;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public static class UseOnToggleExtensions
	{
		public static bool EnabledByToggle(this Type type, IIocConfiguration iocConfiguration)
		{
			var attributes = type.GetCustomAttributes(false);
			return innerEnabledByToggle(iocConfiguration, attributes);
		}

		public static bool EnabledByToggle(this MethodInfo method, IIocConfiguration iocConfiguration)
		{
			var attributes = method.GetCustomAttributes(false);
			return innerEnabledByToggle(iocConfiguration, attributes);
		}

		private static bool innerEnabledByToggle(IIocConfiguration iocConfiguration, object[] attributes)
		{
			var attributesOn = attributes.OfType<EnabledBy>().FirstOrDefault();
			var attributesOff = attributes.OfType<DisabledBy>().FirstOrDefault();

			if (attributesOn == null && attributesOff == null) return true;

			bool resultOn = true;
			bool resultOff = true;

			if (attributesOn != null)
			{
				var togglesOn = attributesOn.Toggles;
				resultOn = togglesOn.All(iocConfiguration.Toggle);
			}
			if (attributesOff != null)
			{
				var togglesOff = attributesOff.Toggles;
				resultOff = !togglesOff.Any(iocConfiguration.Toggle);
			}

			return resultOn && resultOff;
		}
	}
}