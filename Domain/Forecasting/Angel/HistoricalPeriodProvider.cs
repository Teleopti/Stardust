using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class HistoricalPeriodProvider : IHistoricalPeriodProvider
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private const int threeYears = -3;

		public HistoricalPeriodProvider(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
		}

		public DateOnlyPeriod? AvailablePeriod(IWorkload workload)
		{
			var statisticRepository = _repositoryFactory.CreateStatisticRepository();
			var availableHistoricalPeriod = statisticRepository.QueueStatisticsUpUntilDate(workload.QueueSourceCollection);
			if (!availableHistoricalPeriod.HasValue)
				return null;
			var endDate = availableHistoricalPeriod.Value.EndDate;
			var threeYearsBack = endDate.Date.AddYears(threeYears).AddDays(1);
			return threeYearsBack > availableHistoricalPeriod.Value.StartDate.Date ? new DateOnlyPeriod(new DateOnly(threeYearsBack), endDate) : availableHistoricalPeriod.Value;
		}

		public DateOnlyPeriod AvailableIntradayTemplatePeriod(DateOnlyPeriod availablePeriod)
		{
			var endDate = availablePeriod.EndDate;
			var threeMonthsBack = endDate.Date.AddMonths(-3).AddDays(1);
			return threeMonthsBack > availablePeriod.StartDate.Date ? new DateOnlyPeriod(new DateOnly(threeMonthsBack), endDate) : availablePeriod;
		}

		public static Tuple<DateOnlyPeriod,DateOnlyPeriod> DivideIntoTwoPeriods(DateOnlyPeriod availablePeriod)
		{
			if (availablePeriod.StartDate == availablePeriod.EndDate)
				return new Tuple<DateOnlyPeriod, DateOnlyPeriod>(
						new DateOnlyPeriod(availablePeriod.StartDate, availablePeriod.StartDate),
						new DateOnlyPeriod(availablePeriod.StartDate, availablePeriod.StartDate));
			var zeroTime = new DateTime(1, 1, 1);
			var years = (zeroTime + (availablePeriod.EndDate.Date.AddDays(1) - availablePeriod.StartDate.Date)).Year - 1;

			var firstDayInSecondPart = years >= 2
				? new DateOnly(availablePeriod.EndDate.Date.AddYears(-1)).AddDays(1)
				: availablePeriod.StartDate.AddDays((int)Math.Ceiling(new TimeSpan((availablePeriod.EndDate.Date.AddDays(1) - availablePeriod.StartDate.Date).Ticks/2).TotalDays));

			return new Tuple<DateOnlyPeriod, DateOnlyPeriod>(
				new DateOnlyPeriod(availablePeriod.StartDate, firstDayInSecondPart.AddDays(-1)),
				new DateOnlyPeriod(firstDayInSecondPart, availablePeriod.EndDate));
		}
	}
}