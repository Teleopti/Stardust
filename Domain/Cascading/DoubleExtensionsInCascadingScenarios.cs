using System;

namespace Teleopti.Ccc.Domain.Cascading
{
	public static class DoubleExtensionsInCascadingScenarios
	{
		public static bool IsZero(this double value)
		{
			return Math.Abs(value) < 0.0000000000000001d;
		}

		public static bool IsOverstaffed(this double value)
		{
			return value > 0;
		}

		public static bool IsUnderstaffed(this double value)
		{
			return value < 0;
		}
	}
}