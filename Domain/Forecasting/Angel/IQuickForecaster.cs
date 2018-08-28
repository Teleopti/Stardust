using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecaster
	{
		ForecastModel ForecastWorkloadWithinSkill(ISkill skill, IWorkload workload, DateOnlyPeriod futurePeriod,
			IScenario scenario);
	}
}