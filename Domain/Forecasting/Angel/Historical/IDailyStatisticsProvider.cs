using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Historical
{
	public interface IDailyStatisticsProvider
	{
		IEnumerable<DailyStatistic> LoadDailyStatistics(IWorkload workload, DateOnlyPeriod dateRange);
	}
}