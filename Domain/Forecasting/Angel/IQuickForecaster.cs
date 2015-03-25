using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecaster
	{
		void ForecastWorkloadsWithinSkill(ISkill skill, Guid[] workloadIds, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod);
		void ForecastAll(ISkill skill, DateOnlyPeriod futurePeriod, DateOnlyPeriod historicalPeriod);
	}
}