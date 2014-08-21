using System;

namespace Teleopti.Ccc.TestCommon.TestData
{
	// used to prevent common and default named entities be referenced from scenarios.
	// shouldnt they be explicit, if relevant?
	public static class DefaultName
	{
		public static string Make(string name)
		{
			return name + " " + new Random().Next(0, 50) + "~";
		}

		public static string Make()
		{
			return new Random().Next(0, 50) + "~";
		}
	}
}