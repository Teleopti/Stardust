using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public interface IForecastVolumeApplier
	{
		void Apply(IWorkload workload, ITaskOwnerPeriod taskOwnerPeriod, IEnumerable<ITaskOwner> futureWorkloadDays);
	}
}