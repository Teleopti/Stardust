using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecaster
	{
		void ForecastWorkloadsWithinSkill(ISkill skill, ForecastWorkloadInput[] workloads, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriodForForecast, DateOnlyPeriod historicalPeriodForMeasurement);
		void ForecastAll(ISkill skill, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriodForForecast, DateOnlyPeriod historicalPeriodForMeasurement);
	}
}