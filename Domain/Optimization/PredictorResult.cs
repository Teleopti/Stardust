using System;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PredictorResult
	{
		private readonly double _standardDeviation;
		private readonly double _brokenMinimumAgentsInterval;

		public static PredictorResult Create(double currentResult, double predictedResult)
		{
			return new PredictorResult(predictedResult >= currentResult, currentResult, 0);
		}

		public static PredictorResult CreateDueToMinimumAgents(double currentResult, double brokenMinimumAgentsInterval)
		{
			return new PredictorResult(false, currentResult, brokenMinimumAgentsInterval);
		}
		
		private PredictorResult(bool isDefinatlyWorse, double standardDeviation, double brokenMinimumAgentsInterval)
		{
			IsDefinatlyWorse = isDefinatlyWorse;
			_standardDeviation = standardDeviation;
			_brokenMinimumAgentsInterval = brokenMinimumAgentsInterval;
		}
		
		public bool IsDefinatlyWorse { get; }

		public WasReallyBetterResult IsBetterThan(double currentStandardDeviation, double currentBrokenMinimumAgentsInterval)
		{			
			if (Math.Abs(currentBrokenMinimumAgentsInterval - _brokenMinimumAgentsInterval) < 0.01)
			{
				if (currentStandardDeviation < _standardDeviation)
					return WasReallyBetterResult.Yes;
				
				return Math.Abs(currentBrokenMinimumAgentsInterval) < 0.01 ? 
					WasReallyBetterResult.No : 
					WasReallyBetterResult.NoDueToMinimumAgents;
			}

			return _brokenMinimumAgentsInterval > currentBrokenMinimumAgentsInterval ? 
				WasReallyBetterResult.Yes : 
				WasReallyBetterResult.NoDueToMinimumAgents;
		}
	}
}