using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecaster
	{
		void ForecastWorkloadsWithinSkill(ISkill skill, ForecastWorkloadInput[] workloads, DateOnlyPeriod futurePeriod);
		void ForecastAll(ISkill skill, DateOnlyPeriod futurePeriod);
	}
}