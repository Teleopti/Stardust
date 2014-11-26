﻿using System;
using System.Collections.Generic;
using System.Linq;
using DDay.iCal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class FilterHelper
	{
		private readonly IUserTimeZone _userTimeZone;

		public FilterHelper(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
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
							  select new MinMax<DateTime>(
								  selectedDate.Date.Add(TimeSpan.FromHours(start)),
								  selectedDate.Date.Add(TimeSpan.FromHours(end)));
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
			{//only do it for end time filter
				IList<MinMax<DateTime>> oldCopy = new List<MinMax<DateTime>>();
				oldCopy.AddRange(periodsList);

				foreach (var t in oldCopy)
				{// plus 24 hours to get night shifts which may end with tomorrow
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

		public TimeFilterInfo GetFilter(DateOnly selectedDate, string filterStartTimes, string filterEndTimes, bool isDayOff)
		{
			TimeFilterInfo filter;
			if (string.IsNullOrEmpty(filterStartTimes) && string.IsNullOrEmpty(filterEndTimes))
			{
				if (isDayOff)
				{
					filter = new TimeFilterInfo();
					filter.StartTimes = convertStringToUtcTimes(selectedDate, filterStartTimes, false);
					filter.EndTimes = convertStringToUtcTimes(selectedDate, filterEndTimes, false);
					filter.IsDayOff = isDayOff;
				}
				else
				{
					filter = null;
				}
			}
			else
			{
				filter = new TimeFilterInfo();
				filter.StartTimes = convertStringToUtcTimes(selectedDate, filterStartTimes, true);
				filter.EndTimes = convertStringToUtcTimes(selectedDate, filterEndTimes, true, true);
				filter.IsDayOff = isDayOff;
			}
			return filter;
		}
	}
}