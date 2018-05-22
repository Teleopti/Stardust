namespace Teleopti.Ccc.Domain.Optimization
{
	public class WasReallyBetterResult
	{
		private WasReallyBetterResult(bool better, bool minimumAgentsAreCurrentlyBroken)
		{
			Better = better;
			MinimumAgentsAreCurrentlyBroken = minimumAgentsAreCurrentlyBroken;
		}
		
		public bool Better { get; }
		public bool MinimumAgentsAreCurrentlyBroken { get; }

		public static WasReallyBetterResult WasBetter()
		{
			return new WasReallyBetterResult(true, false);
		}

		public static WasReallyBetterResult WasWorse(bool minimumAgentsAreCurrentlyBroken)
		{
			return new WasReallyBetterResult(false, minimumAgentsAreCurrentlyBroken);
		}
	}
}