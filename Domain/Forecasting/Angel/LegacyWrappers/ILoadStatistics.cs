using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers
{
	public interface ILoadStatistics
	{
		IEnumerable<IWorkloadDayBase> LoadWorkloadDay(IWorkload workload, DateOnlyPeriod period);
	}
}