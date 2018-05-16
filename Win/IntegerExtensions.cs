using System;

namespace Teleopti.Ccc.Win
{
	public static class IntegerExtensions
	{
		public static int LimitRange(this int value, int lowerAllowed, int upperAllowed)
		{
			return Math.Max(lowerAllowed, Math.Min(upperAllowed, value));
		}
	}
}