using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Historical
{
	public interface IDailyStatisticsProvider
	{
		IEnumerable<DailyStatistic> LoadDailyStatistics(IWorkload workload, DateOnlyPeriod dateRange);
	}
}