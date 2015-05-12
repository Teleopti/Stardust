using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IHistoricalPeriodProvider
	{
		DateOnlyPeriod PeriodForEvaluate(IWorkload workload);
		DateOnlyPeriod PeriodForForecast();
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
			var endDate=_statisticRepository.QueueStatisticsUpUntilDate(workload);
			return new DateOnlyPeriod(new DateOnly(endDate.Date.AddYears(-2)), endDate);
		}

		public DateOnlyPeriod PeriodForForecast()
		{
			var nowDate = _now.LocalDateOnly();
			return new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-1)), nowDate);
		}
	}
}