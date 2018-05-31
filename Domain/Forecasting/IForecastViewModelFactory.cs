using System;
using Teleopti.Ccc.Domain.Forecasting.Models;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface IForecastViewModelFactory
	{
		WorkloadEvaluateViewModel Evaluate(Guid workloadId);
		WorkloadQueueStatisticsViewModel QueueStatistics(QueueStatisticsInput input);
		WorkloadEvaluateMethodsViewModel EvaluateMethods(Guid workloadId);
	}

	public interface IIntradayPatternViewModelFactory
	{
		IntradayPatternViewModel Create(Guid workloadId);
	}

}