using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IHistoricalPeriodProvider
	{
		DateOnlyPeriod? AvailablePeriod(IWorkload workload);
		DateOnlyPeriod AvailableIntradayTemplatePeriod(DateOnlyPeriod availablePeriod);
	}
}