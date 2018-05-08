using System;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface IForecastResultViewModelFactory
	{
		ForecastModel Create(Guid workloadId, DateOnlyPeriod dateOnlyPeriod, IScenario scenario);
	}
}