using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;

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

		public DateOnly? Find(IMatrixData workingItem, IList<DateOnlyPeriod> splitedWeeksFromSchedulePeriod, IList<DateOnly> bannedDates)
		{
			var numberOfWeeks = splitedWeeksFromSchedulePeriod.Count;
			for (var i = 0; i < numberOfWeeks; i++)
			{
				var leastDaysOffWeek = weekLeastDaysOff(workingItem, splitedWeeksFromSchedulePeriod);
				var scheduleDayCollection = getScheduleDayDataBasedOnPeriod(workingItem.ScheduleDayDataCollection, leastDaysOffWeek, bannedDates);
				var bestSpot = _bestSpotForAddingDayOffFinder.Find(scheduleDayCollection);
				if (bestSpot != null) return bestSpot;
				splitedWeeksFromSchedulePeriod.Remove(leastDaysOffWeek);
			}
			
			return null;
		}

		private DateOnlyPeriod weekLeastDaysOff(IMatrixData workingItem, IEnumerable<DateOnlyPeriod> splitedWeeksFromSchedulePeriod)
		{
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
			return leastDaysOffWeek;
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