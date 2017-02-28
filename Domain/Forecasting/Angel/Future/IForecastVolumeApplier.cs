using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public interface IForecastVolumeApplier
	{
		void Apply(IWorkload workload, ITaskOwnerPeriod historicalData, IEnumerable<ITaskOwner> futureWorkloadDays);
	}
}