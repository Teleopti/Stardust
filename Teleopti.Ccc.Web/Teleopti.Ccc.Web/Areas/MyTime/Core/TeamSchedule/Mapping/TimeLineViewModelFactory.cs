using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TimeLineViewModelFactory : ITimeLineViewModelFactory
	{
		private readonly IUserTimeZone _userTimeZone;

		public TimeLineViewModelFactory(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		public TeamScheduleTimeLineViewModel[] CreateTimeLineHours(DateTimePeriod timeLinePeriod)
		{
			var hourList = new List<TeamScheduleTimeLineViewModel>();
			var startTime = timeLinePeriod.StartDateTime.TimeOfDay;
			var endTime = getEndTime(timeLinePeriod);
			var diff = endTime - startTime;
			var timeZone = _userTimeZone.TimeZone();

			var firstHour = startTime;
			var lastHour = endTime;

			if (startTime.Minutes != 0)
			{
				firstHour = startTime
					.Subtract(new TimeSpan(0, 0, startTime.Minutes, startTime.Seconds, startTime.Milliseconds))
					.Add(TimeSpan.FromHours(1));
			}
			if (endTime.Minutes != 0)
			{
				lastHour = endTime.Subtract(new TimeSpan(0, 0, endTime.Minutes, endTime.Seconds, endTime.Milliseconds));
			}

			var localDate = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.StartDateTime.Date, timeZone);
			for (var hour = firstHour; hour <= lastHour; hour = hour.Add(TimeSpan.FromHours(1)))
			{
				var localDateTime = localDate.Add(hour);
				hourList.Add(new TeamScheduleTimeLineViewModel
				{
					Time = localDateTime.Date.CompareTo(localDate.Date) > 0
						? localDateTime.TimeOfDay.Add(TimeSpan.FromDays(1))
						: localDateTime.TimeOfDay,
					TimeLineDisplay = localDateTime.ToLocalizedTimeFormat(),
					PositionPercentage = diff == TimeSpan.Zero ? 0 : Math.Round((decimal) (hour - startTime).Ticks / diff.Ticks, 4)
				});
			}

			return hourList.ToArray();
		}

		private TimeSpan getEndTime(DateTimePeriod timeLinePeriod)
		{
			var endTime = timeLinePeriod.EndDateTime.TimeOfDay;
			if (!timeLinePeriod.StartDateTime.Date.Equals(timeLinePeriod.EndDateTime.Date))
			{
				endTime = endTime.Add(TimeSpan.FromDays(1));
			}
			return endTime;
		}
	}
}