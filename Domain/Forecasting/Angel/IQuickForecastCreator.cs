using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecastCreator
	{
		ForecastingAccuracy CreateForecastForAllSkills(DateOnlyPeriod futurePeriod);
		ForecastingAccuracy[] CreateForecastForWorkloads(DateOnlyPeriod futurePeriod, Guid[] workloadIds);
	}
}