using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IForecastCreator
	{
		void CreateForecastForWorkload(DateOnlyPeriod futurePeriod, ForecastWorkloadInput workload, IScenario scenario);
	}
}