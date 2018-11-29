using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class TimeFilterHelper : ITimeFilterHelper
	{
		private readonly IUserTimeZone _userTimeZone;

		public TimeFilterHelper(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		public TimeFilterInfo GetFilter(DateOnly selectedDate, string filterStartTimes, string filterEndTimes, bool isDayOff, bool isEmptyDay)
		{
			TimeFilterInfo filter;
			if (string.IsNullOrEmpty(filterStartTimes) && string.IsNullOrEmpty(filterEndTimes))
			{
				if (isDayOff || isEmptyDay)
				{
					filter = new TimeFilterInfo
					{
						StartTimes = convertStringToUtcTimes(selectedDate, filterStartTimes, false),
						EndTimes = convertStringToUtcTimes(selectedDate, filterEndTimes, false),
						IsDayOff = isDayOff,
						IsEmptyDay =  isEmptyDay,
						IsWorkingDay = false
					};
				}
				else
				{
					filter = null;
				}
			}
			else
			{
				filter = new TimeFilterInfo
				{
					StartTimes = convertStringToUtcTimes(selectedDate, filterStartTimes, true),
					EndTimes = convertStringToUtcTimes(selectedDate, filterEndTimes, true, true),
					IsDayOff = isDayOff,
					IsEmptyDay = isEmptyDay,
					IsWorkingDay = true
				};
			}
			return filter;
		}

		public TimeFilterInfo GetTeamSchedulesFilter(DateOnly selectedDate, ScheduleFilter scheduleFilter)
		{
			if (scheduleFilter.IsDayOff)
			{
				return new TimeFilterInfo
				{
					IsDayOff = true
				};
			}

			if (scheduleFilter.OnlyNightShift)
			{
				return new TimeFilterInfo
				{
					OnlyNightShift = true
				};
			}

			if (!string.IsNullOrEmpty(scheduleFilter.FilteredStartTimes) || !string.IsNullOrEmpty(scheduleFilter.FilteredEndTimes))
			{
				return new TimeFilterInfo
				{
					StartTimes = convertStringToUtcTimesForTeamSchedules(selectedDate, scheduleFilter.FilteredStartTimes, true),
					EndTimes = convertStringToUtcTimesForTeamSchedules(selectedDate, scheduleFilter.FilteredEndTimes, true, true)
				};
			}

			return null;
		}

		private IList<DateTimePeriod> convertStringToUtcTimesForTeamSchedules(DateOnly selectedDate, string timesString, bool isFullDay, bool isEndFilter = false)
		{
			return string.IsNullOrEmpty(timesString) ? null : convertStringToUtcTimes(selectedDate, timesString, isFullDay, isEndFilter);
		}

		private IList<DateTimePeriod> convertStringToUtcTimes(DateOnly selectedDate, string timesString, bool isFullDay, bool isEndFilter = false)
		{
			var startTimesx = string.IsNullOrEmpty(timesString) ? new string[] { } : timesString.Split(',');
			var periodsAsString = from t in startTimesx
								  let parts = t.Split('-')
								  let start = parts[0]
								  let end = parts[1]
								  select new
								  {
									  Start = start,
									  End = end
								  };

			var periods = from ps in periodsAsString
						  let start = int.Parse(ps.Start.Split(':')[0])
						  let end = int.Parse(ps.End.Split(':')[0])
						  let startMinutes = int.Parse(ps.Start.Split(':')[1])
						  let endMinutes = int.Parse(ps.End.Split(':')[1])
						  select new MinMax<DateTime>(
							  selectedDate.Date.Add(TimeSpan.FromHours(start)).AddMinutes(startMinutes),
							  selectedDate.Date.Add(TimeSpan.FromHours(end)).AddMinutes(endMinutes));
			var periodsList = periods.ToList();

			if (!periodsList.Any())
			{
				if (isFullDay)
				{
					periodsList.Add(new MinMax<DateTime>(
						selectedDate.Date.Add(TimeSpan.FromHours(0)),
						selectedDate.Date.Add(TimeSpan.FromHours(48))
						));
				}
				else
				{
					periodsList.Add(new MinMax<DateTime>(
						selectedDate.Date.Add(TimeSpan.FromHours(0)),
						selectedDate.Date.Add(TimeSpan.FromHours(0))
						));
				}
			}
			else if (isEndFilter)
			{
				//only do it for end time filter
				var oldCopy = new List<MinMax<DateTime>>(periodsList);
				foreach (var t in oldCopy)
				{
					// plus 24 hours to get night shifts which may end with tomorrow
					var plus = new MinMax<DateTime>(t.Minimum.Add(TimeSpan.FromHours(24)), t.Maximum.Add(TimeSpan.FromHours(24)));
					periodsList.Add(plus);
				}
			}

			var periodsDateUtc = from p in periodsList
								 let start = TimeZoneHelper.ConvertToUtc(p.Minimum, _userTimeZone.TimeZone())
								 let end = TimeZoneHelper.ConvertToUtc(p.Maximum, _userTimeZone.TimeZone())
								 let period = new DateTimePeriod(start, end)
								 select period;

			var utcTimes = periodsDateUtc.ToList();
			return utcTimes;
		}
	}
}