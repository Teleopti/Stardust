using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecaster
	{
		void ForecastForSkill(ISkill skill, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod);
		void ForecastForWorkload(IWorkload workload, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod);
	}
}