namespace Teleopti.Ccc.Domain.Cascading
{
	public static class DoubleExtensionsInCascadingScenarios
	{
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