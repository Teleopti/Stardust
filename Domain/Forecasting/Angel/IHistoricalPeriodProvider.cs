﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IHistoricalPeriodProvider
	{
		DateOnlyPeriod? AvailablePeriod(IWorkload workload);
		DateOnlyPeriod? AvailableIntradayTemplatePeriod(IWorkload workload);
		DateOnlyPeriod AvailableIntradayTemplatePeriod(DateOnlyPeriod availablePeriod);
	}
}