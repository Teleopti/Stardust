using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff
{
	public interface IMissingDayOffBestSpotDecider
	{
		DateOnly? Find(IMatrixData workingItem, IList<DateOnlyPeriod> splitedWeeksFromSchedulePeriod,
			IList<DateOnly> bannedDates);
	}

	public class MissingDayOffBestSpotDecider : IMissingDayOffBestSpotDecider
	{
		private readonly IBestSpotForAddingDayOffFinder _bestSpotForAddingDayOffFinder;

		public MissingDayOffBestSpotDecider(IBestSpotForAddingDayOffFinder bestSpotForAddingDayOffFinder)
		{
			_bestSpotForAddingDayOffFinder = bestSpotForAddingDayOffFinder;
		}

		public DateOnly? Find(IMatrixData workingItem, IList<DateOnlyPeriod> splitedWeeksFromSchedulePeriod,
			IList<DateOnly> bannedDates)
		{
			DateOnly? foundDate = null;
			var leastDaysOff = int.MaxValue;
			var leastDaysOffWeek = new DateOnlyPeriod();
			foreach (var weekPeriod in splitedWeeksFromSchedulePeriod)
			{
				var daysOff = 0;
				foreach (var dateOnly in weekPeriod.DayCollection())
				{
					IScheduleDayData scheduleDay;
					workingItem.TryGetValue(dateOnly, out scheduleDay);
					if (scheduleDay.IsDayOff)
						daysOff++;
				}

				if (daysOff <= leastDaysOff)
				{
					leastDaysOffWeek = weekPeriod;
					leastDaysOff = daysOff;
				}
			}
			var scheduleDayCollection = getScheduleDayDataBasedOnPeriod(workingItem.ScheduleDayDataCollection, leastDaysOffWeek,
				bannedDates);

			foundDate = _bestSpotForAddingDayOffFinder.Find(scheduleDayCollection);

			return foundDate;
		}

		private IList<IScheduleDayData> getScheduleDayDataBasedOnPeriod(IEnumerable<IScheduleDayData> scheduleDayDataCollection, DateOnlyPeriod weekPeriod, IList<DateOnly> alreadyAnalyzedDates)
		{
			var filteredList = new List<IScheduleDayData>();

			foreach (var scheduleDayData in scheduleDayDataCollection)
			{
				var scheduleDayDate = scheduleDayData.DateOnly;
				if (weekPeriod.Contains(scheduleDayDate) && !alreadyAnalyzedDates.Contains(scheduleDayDate))
					filteredList.Add(scheduleDayData);
			}
			return filteredList;
		}
	}
}