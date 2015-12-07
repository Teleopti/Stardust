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
			var attributes = type.GetCustomAttributes(typeof(UseOnToggle), false);
			if (attributes.IsEmpty()) return true;

			var toggle = ((UseOnToggle)attributes.First()).Toggle;
			return iocConfiguration.Toggle(toggle);
		}

		public static bool MethodEnabledByToggle(this MethodInfo method, IIocConfiguration iocConfiguration)
		{
			var attributes = method.GetCustomAttributes(typeof(UseOnToggle), false);
			if (attributes.IsEmpty()) return true;

			var toggle = ((UseOnToggle)attributes.First()).Toggle;
			return iocConfiguration.Toggle(toggle);
		}
	}
}