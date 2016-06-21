using System;

namespace Teleopti.Ccc.Domain.Cascading
{
	public static class DoubleExtensionsInCascadingScenarios
	{
		public const int ValueForClosedSkill = int.MaxValue;
		//put in this namespace and not in "Common" or similar because the diff might be different in other scenarios.
		public static bool IsZero(this double value)
		{
			return Math.Abs(value) < 0.0000000000000001d;
		}

		public static bool IsClosed(this double value)
		{
			return Math.Abs(value - ValueForClosedSkill) < 0.0000000000000001d;
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