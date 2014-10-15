using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers
{
	public class LoadStatistics : ILoadStatistics
	{
		private readonly IStatisticHelperFactory _statisticHelperFactory;

		public LoadStatistics(IStatisticHelperFactory statisticHelperFactory)
		{
			_statisticHelperFactory = statisticHelperFactory;
		}

		public IEnumerable<IWorkloadDayBase> LoadWorkloadDay(IWorkload workload, DateOnlyPeriod period)
		{
			return _statisticHelperFactory.Create().LoadStatisticData(period, workload);
		}
	}
}