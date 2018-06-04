﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Models;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecasterWorkload
	{
		ForecastModel Execute(QuickForecasterWorkloadParams quickForecasterWorkloadParams);
	}
}