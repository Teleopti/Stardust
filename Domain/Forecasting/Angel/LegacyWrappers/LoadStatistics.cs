using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers
{
	public class LoadStatistics : ILoadStatistics
	{
		private readonly IStatisticHelper _statisticHelper;

		public LoadStatistics( IStatisticHelper statisticHelper)
		{
			_statisticHelper = statisticHelper;
		}

		public IEnumerable<IWorkloadDayBase> LoadWorkloadDay(IWorkload workload, DateOnlyPeriod period)
		{
			return _statisticHelper.LoadStatisticData(period, workload);
		}
	}
}