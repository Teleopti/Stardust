using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IForecastingTargetMerger
	{
		void Merge(IList<IForecastingTarget> forecastingTargets, IEnumerable<ITaskOwner> workloadDays);
	}
}