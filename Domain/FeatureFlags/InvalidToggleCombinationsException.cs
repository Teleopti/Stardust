using System;

namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public class InvalidToggleCombinationsException : Exception
	{
		public const string ExMessage = "Toggle combination settings with {0} and {1} is not allowed";
	}
}