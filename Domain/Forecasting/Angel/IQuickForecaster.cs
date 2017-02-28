using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecaster
	{
		void ForecastWorkloadsWithinSkill(ISkill skill, ForecastWorkloadInput[] workloads, DateOnlyPeriod futurePeriod, IScenario scenario);
	}
}