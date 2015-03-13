using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecaster
	{
		double ForecastForSkill(ISkill skill, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod);
		double ForecastForWorkload(IWorkload workload, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod);
		ForecastingAccuracy[] MeasureForecastForSkill(ISkill skill, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod);
	}
}