using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public static class OnlyHandleIfEnabledExtension
	{
		public static bool FeatureIsEnabled(this Type t, IIocConfiguration iocConfiguration)
		{
			var attributes = t.GetCustomAttributes(typeof(OnlyHandleWhenEnabledAttribute), false);
			if (attributes.IsEmpty()) return true;

			var toggle = ((OnlyHandleWhenEnabledAttribute) attributes.First()).Toggle;
			return iocConfiguration.Toggle(toggle);

		}
	}
}