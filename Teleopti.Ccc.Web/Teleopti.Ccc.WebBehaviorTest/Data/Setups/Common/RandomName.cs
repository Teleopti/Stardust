using System;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common
{

	// used to prevent common entities be referenced from scenarios.
	// shouldnt they be explicit, if relevant?
	public static class RandomName
	{
		public static string Make(string name)
		{
			return "Common site " + new Random().Next(0, 50) + "~";
		}
	}
}