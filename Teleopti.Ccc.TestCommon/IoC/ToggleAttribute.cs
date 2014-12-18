using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ToggleAttribute : Attribute
	{
		public Toggles Toggle { get; set; }

		public ToggleAttribute(Toggles toggle)
		{
			Toggle = toggle;
		}
	}
}