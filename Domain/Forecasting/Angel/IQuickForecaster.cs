﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecaster
	{
		IList<ForecastResultModel> ForecastWorkloadsWithinSkill(ISkill skill, ForecastWorkloadInput workload, DateOnlyPeriod futurePeriod, IScenario scenario);
	}
}