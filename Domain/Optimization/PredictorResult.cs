namespace Teleopti.Ccc.Domain.Optimization
{
	public class PredictorResult
	{
		private readonly double _standardDeviation;
		private readonly int _brokenMinimumAgentsInterval;

		public static PredictorResult Create(double currentResult, double predictedResult)
		{
			return new PredictorResult(predictedResult >= currentResult, currentResult, 0);
		}

		public static PredictorResult CreateDueToMinimumAgents(double currentResult, int brokenMinimumAgentsInterval)
		{
			return new PredictorResult(false, currentResult, brokenMinimumAgentsInterval);
		}
		
		private PredictorResult(bool isDefinatlyWorse, double standardDeviation, int brokenMinimumAgentsInterval)
		{
			IsDefinatlyWorse = isDefinatlyWorse;
			_standardDeviation = standardDeviation;
			_brokenMinimumAgentsInterval = brokenMinimumAgentsInterval;
		}
		
		public bool IsDefinatlyWorse { get; }

		public WasReallyBetterResult IsBetterThan(double currentStandardDeviation, int currentBrokenMinimumAgentsInterval)
		{
			if (currentBrokenMinimumAgentsInterval > _brokenMinimumAgentsInterval)
				return WasReallyBetterResult.NoDueToMinimumAgents;

			if (_brokenMinimumAgentsInterval > currentBrokenMinimumAgentsInterval)
				return WasReallyBetterResult.Yes;
			
			return currentStandardDeviation < _standardDeviation ? 
				WasReallyBetterResult.Yes : 
				WasReallyBetterResult.No;
		}
	}
}