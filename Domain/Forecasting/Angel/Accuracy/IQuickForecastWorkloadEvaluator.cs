using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IQuickForecastWorkloadEvaluator
	{
		ForecastingAccuracy Measure(IWorkload workload, DateOnlyPeriod historicalPeriod);
	}
}