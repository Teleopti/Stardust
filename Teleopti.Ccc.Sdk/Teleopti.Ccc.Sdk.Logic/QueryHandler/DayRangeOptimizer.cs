using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public class DayRangeOptimizer
	{
		public static DayProjectionPage ReduceAndPage(List<PersonDayProjectionChanged> dayProjectionChanges, int pageNo, int pageSizeOfDays)
		{ 
			var optimizedChangedDayRanges = Reduce(dayProjectionChanges);
			return PageByProjectionDays(optimizedChangedDayRanges, pageNo, pageSizeOfDays);
		}

		public static List<PersonDayProjectionChanged> Reduce(List<PersonDayProjectionChanged> dayProjectionChanges)
		{
			var peopleChangeSet = dayProjectionChanges
				.GroupBy(c => c.PersonId);
			var reducedChanges = new List<PersonDayProjectionChanged>();

			Func<List<DatetimeMeta>, PersonDayProjectionChanged, List<DatetimeMeta>> reduceToDateTimeMeta = (dateList, change) =>
			{
				dateList.Add(new DatetimeMeta { Date = change.StartDate.Date, IsStartDate = true });
				dateList.Add(new DatetimeMeta { Date = change.EndDate.Date, IsEndDate = true });
				return dateList;
			};

			foreach (var personChangeSet in peopleChangeSet)
			{
				var personChangedDates = personChangeSet.ToList()
					.Aggregate(new List<DatetimeMeta>(), reduceToDateTimeMeta)
					.OrderBy(dtm => dtm.Date)
					.ToList();
				var opened = 0;
				var i = 0;

				while (true)
				{
					if (i == personChangedDates.Count() && opened == 0) break;

					if (personChangedDates[i].IsStartDate)
					{
						if (opened == 0 && i >= 2)
						{
							var prev = personChangedDates[i - 1];
							var prevWasYesterday = prev.Date == personChangedDates[i].Date.AddDays(-1);
							var prevWasToday = prev.Date == personChangedDates[i].Date;
							if (prevWasYesterday || prevWasToday)
							{
								personChangedDates.RemoveAt(i);
								personChangedDates.RemoveAt(i - 1);
								i -= 2;
							}
						}

						opened++;
					}
					else if (personChangedDates[i].IsEndDate)
					{
						if (opened >= 2)
						{
							personChangedDates.RemoveAt(i);
							personChangedDates.RemoveAt(i - 1);
							i -= 2;
						}

						opened--;
					}

					i++;
				}

				for (int i2 = 0; i2 < personChangedDates.Count(); i2 += 2)
				{
					reducedChanges.Add(new PersonDayProjectionChanged(personChangeSet.Key, personChangedDates[i2].Date.ToDateOnly(), personChangedDates[i2 + 1].Date.ToDateOnly()));
				}
			}

			return reducedChanges;
		}

		public static DayProjectionPage PageByProjectionDays(List<PersonDayProjectionChanged> dayProjectionChanges, int pageNo, int pageSizeOfDays)
		{
			if (pageNo == 0) // Dont page.
			{
				return new DayProjectionPage(dayProjectionChanges.Sum(d => d.DaysInRange), 1, dayProjectionChanges);
			}
			if (pageNo <= 0 || pageSizeOfDays <= 0)
			{
				throw new FaultException($"Invalid page input. Page parameters must be larger or equal to '0'. {nameof(pageNo)} = '{pageNo}', {nameof(pageSizeOfDays)} = '{pageSizeOfDays}'");
			}

			dayProjectionChanges = dayProjectionChanges.OrderBy(x => x.StartDate).ThenBy(y => y.PersonId).ToList();
			var pageRanges = new List<MinMax<int>>();
			var currentPageStartIndex = 0;
			var currentPageNoDays = 0;
			var totalNoDays = 0;

			for (int currentPageIndex = 0; currentPageIndex < dayProjectionChanges.Count; currentPageIndex++)
			{
				currentPageNoDays += dayProjectionChanges[currentPageIndex].DaysInRange;
				totalNoDays += dayProjectionChanges[currentPageIndex].DaysInRange;
				if (currentPageNoDays >= pageSizeOfDays)
				{
					pageRanges.Add(new MinMax<int>(currentPageStartIndex, currentPageIndex));
					currentPageNoDays = 0;
					currentPageStartIndex = currentPageIndex + 1;
				}
			}

			if (currentPageNoDays != 0)
			{
				pageRanges.Add(new MinMax<int>(currentPageStartIndex, dayProjectionChanges.Count - 1));
			}

			if (pageNo > pageRanges.Count)
			{
				return new DayProjectionPage(totalNoDays, pageRanges.Count);
			}

			var pageProjections = dayProjectionChanges.GetRange(pageRanges[pageNo - 1].Minimum, pageRanges[pageNo - 1].Maximum - pageRanges[pageNo - 1].Minimum + 1);

			return new DayProjectionPage(totalNoDays, pageRanges.Count, pageProjections);
		}

		internal class DatetimeMeta
		{
			public DateTime Date;
			public bool IsStartDate { get; set; }
			public bool IsEndDate { get; set; }
		}
	}

	public class RangeOptimizerOldAlternative
	{
		public static List<PersonDayProjectionChanged> Reduce(List<PersonDayProjectionChanged> toReduce)
		{
			var reduction = new List<PersonDayProjectionChanged>();
			foreach (var person in toReduce.Select(p => p.PersonId).Distinct())
			{
				var currentRanges = toReduce.Where(p => p.PersonId == person).ToList();
				var timeUnits = currentRanges.Select(x => new TimeUnit(x.StartDate.Date, true)).ToList();
				timeUnits.AddRange(currentRanges.Select(x => new TimeUnit(x.EndDate.Date, false)));
				timeUnits = timeUnits.OrderBy(t => t.DateTime).ToList(); //.ThenByDescending(t => t.IsStartTime).ToList(); 

				var trackStack = new TimeUnit[timeUnits.Count];

				int stackIndex = 0;
				for (int i = 0; i < timeUnits.Count; i++)
				{
					if (stackIndex < 2)
					{
						trackStack[stackIndex++] = timeUnits[i];
					}
					else if (timeUnits[i].IsStartTime)
					{
						if (trackStack[stackIndex - 1].IsStartTime == false &&
							timeUnits[i].DateTime.AddDays(-1) == trackStack[stackIndex - 1].DateTime) // New start same as last endtime?
						{
							stackIndex--;
						}
						else
						{
							trackStack[stackIndex++] = timeUnits[i];
						}
					}
					else // is endtime, si >=2
					{
						if (trackStack[stackIndex - 1].IsStartTime && trackStack[stackIndex - 2].IsStartTime)
						{
							stackIndex--;
						}
						else
						{
							trackStack[stackIndex++] = timeUnits[i];
						}
					}
				}

				for (int r = 0; r < stackIndex; r += 2)
				{
					reduction.Add(new PersonDayProjectionChanged(person, trackStack[r].DateTime.ToDateOnly(), trackStack[r + 1].DateTime.ToDateOnly()));
				}
			}

			return reduction;
		}

		[DebuggerDisplay("{DateTime} - {IsStartTime}")]
		private struct TimeUnit
		{
			public DateTime DateTime;
			public bool IsStartTime;

			public TimeUnit(DateTime dateTime, bool isStartTime)
			{
				DateTime = dateTime;
				IsStartTime = isStartTime;
			}
		}
	}
}
