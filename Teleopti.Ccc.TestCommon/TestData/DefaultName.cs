using System;
using System.Linq;

namespace Teleopti.Ccc.TestCommon.TestData
{
	// used to prevent common and default named entities be referenced from scenarios.
	// shouldnt they be explicit, if relevant?
	public static class DefaultName
	{
		public static string Make(string name)
		{
			return name + " " + randomString(6) + "~";
		}

		public static string Make()
		{
			return randomString(6) + "~";
		}

		private static readonly Random Random = new Random();

		private static string randomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(
				Enumerable.Repeat(chars, length)
						  .Select(s => s[Random.Next(s.Length)])
						  .ToArray());
		}
	}
}