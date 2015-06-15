using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface IForecastEvaluator
	{
		WorkloadForecastViewModel Evaluate(EvaluateInput input);
		WorkloadQueueStatisticsViewModel QueueStatistics(QueueStatisticsInput input);
	}

}