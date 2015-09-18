using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface IForecastResultViewModelFactory
	{
		WorkloadForecastResultViewModel Create(Guid workloadId, DateOnlyPeriod dateOnlyPeriod, IScenario scenario);
	}
}