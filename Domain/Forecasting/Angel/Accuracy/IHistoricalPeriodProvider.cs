using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IHistoricalPeriodProvider
	{
		DateOnlyPeriod PeriodForEvaluate(IWorkload workload);
		DateOnlyPeriod PeriodForForecast(IWorkload workload);
	}

	public class HistoricalPeriodProvider : IHistoricalPeriodProvider
	{
		private readonly INow _now;
		private readonly IStatisticRepository _statisticRepository;

		public HistoricalPeriodProvider(INow now, IStatisticRepository statisticRepository)
		{
			_now = now;
			_statisticRepository = statisticRepository;
		}

		public DateOnlyPeriod PeriodForEvaluate(IWorkload workload)
		{
			var endDate = _statisticRepository.QueueStatisticsUpUntilDate(workload.QueueSourceCollection);
			return endDate.HasValue
				? new DateOnlyPeriod(new DateOnly(endDate.Value.Date.AddYears(-2)), endDate.Value)
				: new DateOnlyPeriod(_now.LocalDateOnly(), _now.LocalDateOnly());
		}

		public DateOnlyPeriod PeriodForForecast(IWorkload workload)
		{
			var endDate = _statisticRepository.QueueStatisticsUpUntilDate(workload.QueueSourceCollection);
			return endDate.HasValue
				? new DateOnlyPeriod(new DateOnly(endDate.Value.Date.AddYears(-2)), endDate.Value)
				: new DateOnlyPeriod(_now.LocalDateOnly(), _now.LocalDateOnly());
		}
	}
}