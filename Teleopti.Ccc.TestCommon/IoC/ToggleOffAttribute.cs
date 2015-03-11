using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class ToggleOffAttribute : Attribute
	{
		public Toggles Toggle { get; set; }

		public ToggleOffAttribute(Toggles toggle)
		{
			Toggle = toggle;
		}
	}
}