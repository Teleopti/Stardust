namespace Teleopti.Ccc.Domain.Optimization
{
	public class WasReallyBetterResult
	{
		private WasReallyBetterResult(bool better, bool minimumAgentsAreCurrentlyBroken, bool failedWhenPlacingShift)
		{
			Better = better;
			MinimumAgentsAreCurrentlyBroken = minimumAgentsAreCurrentlyBroken;
			FailedWhenPlacingShift = failedWhenPlacingShift;
		}
		
		public bool Better { get; }
		public bool MinimumAgentsAreCurrentlyBroken { get; }
		public bool FailedWhenPlacingShift { get; }

		public static WasReallyBetterResult WasBetter()
		{
			return new WasReallyBetterResult(true, false, false);
		}

		public static WasReallyBetterResult WasWorse(bool minimumAgentsAreCurrentlyBroken, bool failedWhenPlacingShift)
		{
			return new WasReallyBetterResult(false, minimumAgentsAreCurrentlyBroken, failedWhenPlacingShift);
		}
	}
}