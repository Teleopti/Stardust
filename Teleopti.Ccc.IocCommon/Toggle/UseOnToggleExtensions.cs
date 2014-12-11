using System;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public static class UseOnToggleExtensions
	{
		public static bool EnabledByToggle(this Type t, IIocConfiguration iocConfiguration)
		{
			var attributes = t.GetCustomAttributes(typeof(UseOnToggle), false);
			if (attributes.IsEmpty()) return true;

			var toggle = ((UseOnToggle) attributes.First()).Toggle;
			return iocConfiguration.Toggle(toggle);

		}

		public static bool EnabledByToggle(this Type t, IToggleManager toggleManager)
		{
			var attributes = t.GetCustomAttributes(typeof(UseOnToggle), false);
			if (attributes.IsEmpty()) return true;

			var toggle = ((UseOnToggle)attributes.First()).Toggle;
			return toggleManager.IsEnabled(toggle);

		}
	}
}