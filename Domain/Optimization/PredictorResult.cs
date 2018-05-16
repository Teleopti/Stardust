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

		public static PredictorResult CreateBreaksDueToMinimumAgents()
		{
			return new PredictorResult(true, 10, true);
		}
		
		private PredictorResult(bool isBetter, double standardDeviation, bool breaksMinimumAgents)
		{
			IsBetter = isBetter;
			_standardDeviation = standardDeviation;
			_breaksMinimumAgents = breaksMinimumAgents;
		}
		
		public bool IsBetter { get; }

		public bool IsBetterThan(double currentStandardDeviation)
		{
			return _breaksMinimumAgents || currentStandardDeviation < _standardDeviation;
		}
	}
}