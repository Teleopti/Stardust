using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface IForecastViewModelFactory
	{
		WorkloadEvaluateViewModel Evaluate(EvaluateInput input);
		WorkloadQueueStatisticsViewModel QueueStatistics(QueueStatisticsInput input);
		WorkloadEvaluateMethodsViewModel EvaluateMethods(EvaluateMethodsInput input);
	}


	public interface IIntradayPatternViewModelFactory
	{
		IntradayPatternViewModel Create(IntradayPatternInput input);
	}

}