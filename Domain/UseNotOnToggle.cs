using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public sealed class UseNotOnToggle : Attribute
	{
		public UseNotOnToggle(Toggles toggle)
		{
			Toggle = toggle;
		}

		public Toggles Toggle { get; private set; }
	}

}