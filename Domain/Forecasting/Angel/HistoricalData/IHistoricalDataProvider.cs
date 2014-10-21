using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData
{
	public interface IHistoricalDataProvider
	{
		IEnumerable<DailyStatistic> Fetch(IWorkload workload, DateOnlyPeriod period);
	}
}