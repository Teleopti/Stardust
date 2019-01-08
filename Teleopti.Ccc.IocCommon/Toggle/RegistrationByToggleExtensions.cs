using System;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public static class RegistrationByToggleExtensions
	{
		public static bool EnabledByToggle(this Type type, IocConfiguration config) => type.EnabledByToggle(config.IsToggleEnabled);

		public static bool EnabledByToggle(this Type type, Func<Toggles, bool> toggles)
		{
			var attributes = type.GetCustomAttributes(false);
			return innerEnabledByToggle(toggles, attributes);
		}

		public static bool EnabledByToggle(this MethodInfo method, Func<Toggles, bool> toggles)
		{
			var attributes = method.GetCustomAttributes(false);
			return innerEnabledByToggle(toggles, attributes);
		}

		private static bool innerEnabledByToggle(Func<Toggles, bool> toggles, object[] attributes)
		{
			var attributesOn = attributes.OfType<EnabledBy>().FirstOrDefault();
			var attributesOff = attributes.OfType<DisabledBy>().FirstOrDefault();

			if (attributesOn == null && attributesOff == null) return true;

			var resultOn = true;
			var resultOff = true;

			if (attributesOn != null)
			{
				var togglesOn = attributesOn.Toggles;
				resultOn = togglesOn.All(toggles);
			}

			if (attributesOff != null)
			{
				var togglesOff = attributesOff.Toggles;
				resultOff = !togglesOff.Any(toggles);
			}

			return resultOn && resultOff;
		}
	}
}