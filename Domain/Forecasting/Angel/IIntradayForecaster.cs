using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IIntradayForecaster
	{
		void Apply(IWorkload workload, DateOnlyPeriod templatePeriod, IEnumerable<ITaskOwner> futureWorkloadDays);
	}
}