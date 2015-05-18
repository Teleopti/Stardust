using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class HistoricalPeriodProvider : IHistoricalPeriodProvider
	{
		private readonly INow _now;
		private readonly IStatisticRepository _statisticRepository;

		public HistoricalPeriodProvider(INow now, IStatisticRepository statisticRepository)
		{
			_now = now;
			_statisticRepository = statisticRepository;
		}

		public DateOnlyPeriod PeriodForForecast(IWorkload workload)
		{
			var availableHistoricalPeriod = _statisticRepository.QueueStatisticsUpUntilDate(workload.QueueSourceCollection);
			return availableHistoricalPeriod ?? new DateOnlyPeriod(_now.LocalDateOnly(), _now.LocalDateOnly());
		}

		public DateOnlyPeriod PeriodForDisplay(IWorkload workload)
		{
			var availableHistoricalPeriod = _statisticRepository.QueueStatisticsUpUntilDate(workload.QueueSourceCollection);
			if (availableHistoricalPeriod.HasValue)
			{
				var endDate = availableHistoricalPeriod.Value.EndDate;
				return new DateOnlyPeriod(new DateOnly(endDate.Date.AddYears(-1)), endDate);
			}
			return new DateOnlyPeriod(_now.LocalDateOnly(), _now.LocalDateOnly());
		}
	}
}