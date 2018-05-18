namespace Teleopti.Ccc.Domain.Optimization
{
	public class PredictorResult
	{
		private readonly bool _breaksMinimumAgents;
		private readonly double _standardDeviation;

		public static PredictorResult Create(double currentResult, double predictedResult)
		{
			return new PredictorResult(predictedResult < currentResult, currentResult, false);
		}

		public static PredictorResult CreateBreaksDueToMinimumAgents(double currentResult)
		{
			return new PredictorResult(true, currentResult, true);
		}
		
		private PredictorResult(bool isBetter, double standardDeviation, bool breaksMinimumAgents)
		{
			IsBetter = isBetter;
			_standardDeviation = standardDeviation;
			_breaksMinimumAgents = breaksMinimumAgents;
		}
		
		public bool IsBetter { get; }

		public WasReallyBetterResult IsBetterThan(bool currentlyBreakingMinimumAgents, double currentStandardDeviation)
		{
			if (currentlyBreakingMinimumAgents && !_breaksMinimumAgents)
				return WasReallyBetterResult.NoDueToMinimumAgents;

			if (!currentlyBreakingMinimumAgents && _breaksMinimumAgents)
				return WasReallyBetterResult.Yes;
			
			return currentStandardDeviation < _standardDeviation ? 
				WasReallyBetterResult.Yes : 
				WasReallyBetterResult.No;
		}
	}
}