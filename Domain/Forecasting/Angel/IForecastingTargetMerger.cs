using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IForecastingTargetMerger
	{
		void Merge(IEnumerable<IForecastingTarget> forecastingTargets, IEnumerable<ITaskOwner> workloadDays);
	}
}