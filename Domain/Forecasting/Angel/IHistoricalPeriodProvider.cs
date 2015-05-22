using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IHistoricalPeriodProvider
	{
		DateOnlyPeriod PeriodForForecast(IWorkload workload);
		DateOnlyPeriod PeriodForDisplay(IWorkload workload);
	}
}