using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IForecastCreator
	{
		ForecastModel CreateForecastForWorkload(DateOnlyPeriod futurePeriod, ForecastWorkloadInput workload, IScenario scenario);
	}
}