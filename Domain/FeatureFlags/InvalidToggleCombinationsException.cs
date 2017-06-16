using System;

namespace Teleopti.Ccc.Domain.FeatureFlags
{
	public class InvalidToggleCombinationsException : Exception
	{
		public const string ExMessage = "Toggle combination settings with {0} and {1} is not allowed";

		public InvalidToggleCombinationsException(Toggles toggle1, Toggles toggle2) : base(string.Format(ExMessage, toggle1, toggle2))
		{
		
		}	
	}
}