using System;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class HistoricalPeriodProvider : IHistoricalPeriodProvider
	{
		private readonly INow _now;
		private readonly IStatisticRepository _statisticRepository;
		private const int threeYears = -3;

		public HistoricalPeriodProvider(INow now, IStatisticRepository statisticRepository)
		{
			_now = now;
			_statisticRepository = statisticRepository;
		}

		public DateOnlyPeriod AvailablePeriod(IWorkload workload)
		{
			var availableHistoricalPeriod = _statisticRepository.QueueStatisticsUpUntilDate(workload.QueueSourceCollection);
			if (!availableHistoricalPeriod.HasValue)
				return new DateOnlyPeriod(_now.LocalDateOnly(), _now.LocalDateOnly());
			var endDate = availableHistoricalPeriod.Value.EndDate;
			var threeYearsBack = endDate.Date.AddYears(threeYears).AddDays(1);
			return threeYearsBack > availableHistoricalPeriod.Value.StartDate.Date ? new DateOnlyPeriod(new DateOnly(threeYearsBack), endDate) : availableHistoricalPeriod.Value;
		}

		public static DateOnly DivideIntoTwoPeriods(DateOnlyPeriod availablePeriod)
		{
			var zeroTime = new DateTime(1, 1, 1);
			var years = (zeroTime + (availablePeriod.EndDate.Date.AddDays(1) - availablePeriod.StartDate.Date)).Year - 1;
			if (years >= 2)
				return new DateOnly(availablePeriod.EndDate.Date.AddYears(-1));
			var firstPart = new TimeSpan((availablePeriod.EndDate.Date.AddDays(1) - availablePeriod.StartDate.Date).Ticks/2).Days;
			return availablePeriod.StartDate.AddDays(firstPart);
		}
	}
}