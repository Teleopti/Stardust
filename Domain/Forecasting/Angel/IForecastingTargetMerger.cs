using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IForecastingTargetMerger
	{
		void Merge(IEnumerable<IForecastingTarget> forecastingTargets, IEnumerable<ITaskOwner> workloadDays);
	}
}