using System;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public static class UseOnToggleExtensions
	{
		public static bool TypeEnabledByToggle(this Type type, IIocConfiguration iocConfiguration)
		{
			var attributesOn = type.GetCustomAttributes(typeof(EnabledBy), false);
			var attributesOff = type.GetCustomAttributes(typeof(DisabledBy), false);

			if (attributesOn.IsEmpty() && attributesOff.IsEmpty()) return true;

			bool resultOn = true;
			bool resultOff = true;

			if (attributesOn.Any())
			{
				var togglesOn = ((EnabledBy)attributesOn.First()).Toggles;
				resultOn = togglesOn.All(iocConfiguration.Toggle);
			}
			if (attributesOff.Any())
			{
				var togglesOff = ((DisabledBy)attributesOff.First()).Toggles;
				resultOff = !togglesOff.Any(iocConfiguration.Toggle); 
			}
			
			return resultOn && resultOff;
		}

		public static bool MethodEnabledByToggle(this MethodInfo method, IIocConfiguration iocConfiguration)
		{
			var attributesOn = method.GetCustomAttributes(typeof(EnabledBy), false);
			var attributesOff = method.GetCustomAttributes(typeof(DisabledBy), false);

			if (attributesOn.IsEmpty() && attributesOff.IsEmpty()) return true;

			bool resultOn = true;
			bool resultOff = true;

			if (attributesOn.Any())
			{
				var togglesOn = ((EnabledBy)attributesOn.First()).Toggles;
				resultOn = togglesOn.All(iocConfiguration.Toggle);
			}
			if (attributesOff.Any())
			{
				var togglesOff = ((DisabledBy)attributesOff.First()).Toggles;
				resultOff = !togglesOff.Any(iocConfiguration.Toggle);
			}

			return resultOn && resultOff;
		}
	}
}