using System;
using System.Linq;

namespace Teleopti.Ccc.TestCommon.TestData
{
	// used to prevent common and default named entities be referenced from scenarios.
	// shouldnt they be explicit, if relevant?
	public static class RandomName
	{
		private static readonly Random random = new Random();

		public static string Make(string baseName)
		{
			return baseName + " " + randomString(6) + "~";
		}

		public static string Make()
		{
			return randomString(6) + "~";
		}

		private static string randomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(
				Enumerable.Repeat(chars, length)
						  .Select(s => s[random.Next(s.Length)])
						  .ToArray());
		}
	}
}