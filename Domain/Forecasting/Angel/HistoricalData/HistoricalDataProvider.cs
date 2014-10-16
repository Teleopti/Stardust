using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData
{
	public class HistoricalDataProvider : IHistoricalDataProvider
	{
		private readonly ILoadStatistics _loadStatistics;
		private readonly IValidatedVolumeDayRepository _validatedVolumeDayRepository;

		public HistoricalDataProvider(ILoadStatistics loadStatistics, IValidatedVolumeDayRepository validatedVolumeDayRepository)
		{
			_loadStatistics = loadStatistics;
			_validatedVolumeDayRepository = validatedVolumeDayRepository;
		}

		public IEnumerable<IValidatedVolumeDay> Calculate(IWorkload workload, DateOnlyPeriod period)
		{
			var statistics = _loadStatistics.LoadWorkloadDay(workload, period);
			var validated = _validatedVolumeDayRepository == null ? 
				Enumerable.Empty<IValidatedVolumeDay>() : 
				_validatedVolumeDayRepository.FindRange(period, workload);

			var ret = new List<IValidatedVolumeDay>();
			foreach (var statistic in statistics)
			{
				var toBeReturned = validated.FirstOrDefault(v => v.VolumeDayDate == statistic.CurrentDate) ??
				                   new ValidatedVolumeDay(workload, statistic.CurrentDate);
				toBeReturned.TaskOwner = statistic;
				ret.Add(toBeReturned);
			}
			return ret;
		}
	}
}