using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecaster
	{
		void ForecastForWorkload(IWorkload workload, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod);
	}
}