using System;

namespace Teleopti.Ccc.Domain.FeatureFlags
{
	[AttributeUsage(AttributeTargets.All | AttributeTargets.Method, AllowMultiple = true)]
	public class RemoveMeWithToggleAttribute : Attribute
	{
		public RemoveMeWithToggleAttribute(params Toggles[] toggles)
		{ 
		}

		public RemoveMeWithToggleAttribute(string comment, params Toggles[] toggles)
		{

		}

		public static void This(params Toggles[] toggles)
		{
		}
	}
}