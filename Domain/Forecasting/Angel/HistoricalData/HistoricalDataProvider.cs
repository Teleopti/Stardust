using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData
{
	public class HistoricalDataProvider : IHistoricalDataProvider
	{
		private readonly ILoadStatistics _loadStatistics;

		public HistoricalDataProvider(ILoadStatistics loadStatistics)
		{
			_loadStatistics = loadStatistics;
		}

		public IEnumerable<IValidatedVolumeDay> Calculate(IWorkload workload, DateOnlyPeriod period)
		{
			var statistics = _loadStatistics.LoadWorkloadDay(workload, period);
			var ret = new List<IValidatedVolumeDay>();
			foreach (var statistic in statistics)
			{
				var validatedVolumeDay = new ValidatedVolumeDay(workload, statistic.CurrentDate) {TaskOwner = statistic};
				ret.Add(validatedVolumeDay);
			}
			return ret;
		}
	}
}