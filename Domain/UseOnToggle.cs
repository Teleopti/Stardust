using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class UseOnToggle : Attribute
	{
		public UseOnToggle(Toggles toggle)
		{
			Toggle = toggle;
		}

		public Toggles Toggle { get; private set; }
	}

}