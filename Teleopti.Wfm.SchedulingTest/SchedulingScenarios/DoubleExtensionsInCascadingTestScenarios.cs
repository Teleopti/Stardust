using System;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public static class DoubleExtensionsInCascadingTestScenarios
	{
		public static bool IsZero(this double value)
		{
			return Math.Abs(value) < 0.0000000000000001d;
		}
	}
}