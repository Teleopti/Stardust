using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IHistoricalPeriodProvider
	{
		DateOnlyPeriod AvailablePeriod(IWorkload workload);
	}
}