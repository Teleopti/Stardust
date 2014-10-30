using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface IForecastHistoricalPeriodCalculator
	{
		DateOnlyPeriod HistoricalPeriod(DateOnlyPeriod futurePeriod);
	}
}