using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Core.Extensions;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TimeLineViewModelFactory : ITimeLineViewModelFactory
	{
		private readonly IUserTimeZone _userTimeZone;

		public TimeLineViewModelFactory(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		public TeamScheduleTimeLineViewModel[] CreateTimeLineHours(DateTimePeriod timeLinePeriodInUtc, DateOnly viewDate)
		{
			var hourList = new List<TeamScheduleTimeLineViewModel>();
			var startTime = timeLinePeriodInUtc.StartDateTime;
			var endTime = timeLinePeriodInUtc.EndDateTime;
			
			var timeZone = _userTimeZone.TimeZone();

			var firstHour = startTime.Add(TimeSpan.FromMinutes(15));
			var lastHour = endTime.Subtract(TimeSpan.FromMinutes(15));

			if (firstHour.Minute != 0)
			{
				firstHour = startTime
					.Subtract(new TimeSpan(0, 0, startTime.Minute, startTime.Second, startTime.Millisecond));
			}
			if (lastHour.Minute != 0)
			{
				lastHour = endTime.Subtract(new TimeSpan(0, 0, endTime.Minute, endTime.Second, endTime.Millisecond));
			}

			var diff = endTime - firstHour;
			for (var hour = firstHour; hour <= lastHour; hour = hour.Add(TimeSpan.FromHours(1)))
			{
				var time = TimeZoneHelper.ConvertFromUtc(hour, timeZone);
				hourList.Add(new TeamScheduleTimeLineViewModel
				{
					Time = time,
					TimeLineDisplay = time.ToLocalizedDateTimeFormatWithTSpliting(),
					PositionPercentage = diff == TimeSpan.Zero ? 0 : Math.Round((decimal) (hour - firstHour).Ticks / diff.Ticks, 4)
				});
			}

			return hourList.ToArray();
		}
	}
}