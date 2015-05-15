using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IHistoricalPeriodProvider
	{
		DateOnlyPeriod PeriodForEvaluate(IWorkload workload);
		DateOnlyPeriod PeriodForForecast(IWorkload workload);
		DateOnlyPeriod PeriodForDisplay(IWorkload workload);
	}
}