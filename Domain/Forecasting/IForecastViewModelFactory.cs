using System;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Web.Areas.Forecasting.Models;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface IForecastViewModelFactory
	{
		WorkloadEvaluateViewModel Evaluate(Guid workloadId);
		WorkloadQueueStatisticsViewModel QueueStatistics(QueueStatisticsInput input);
		WorkloadEvaluateMethodsViewModel EvaluateMethods(EvaluateMethodsInput input);
	}


	public interface IIntradayPatternViewModelFactory
	{
		IntradayPatternViewModel Create(Guid workloadId);
	}

}