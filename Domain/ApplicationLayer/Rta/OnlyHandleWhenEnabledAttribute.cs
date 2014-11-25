using System;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class OnlyHandleWhenEnabledAttribute : Attribute
	{
		public OnlyHandleWhenEnabledAttribute(Toggles toggle)
		{
			Toggle = toggle;
		}
		public Toggles Toggle { get; private  set; }
	}
}