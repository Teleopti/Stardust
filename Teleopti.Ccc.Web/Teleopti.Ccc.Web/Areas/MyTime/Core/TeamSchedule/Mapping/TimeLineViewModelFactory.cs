using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TimeLineViewModelFactoryToggle75989Off : ITimeLineViewModelFactory
	{
		private readonly ICreateHourText _createHourText;
		private readonly IUserTimeZone _userTimeZone;

		public TimeLineViewModelFactoryToggle75989Off(ICreateHourText createHourText, IUserTimeZone userTimeZone)
		{
			_createHourText = createHourText;
			_userTimeZone = userTimeZone;
		}

		public TimeLineViewModel[] CreateTimeLineHours(DateTimePeriod timeLinePeriod)
		{
			var hourList = new List<TeamScheduleTimeLineViewModelToggle75989Off>();
			TeamScheduleTimeLineViewModelToggle75989Off lastHour = null;
			var shiftStartRounded = timeLinePeriod.StartDateTime;
			var shiftEndRounded = timeLinePeriod.EndDateTime;
			var timeZone = _userTimeZone.TimeZone();

			if (timeLinePeriod.StartDateTime.Minute != 0)
			{
				var lengthInMinutes = 60 - timeLinePeriod.StartDateTime.Minute;
				hourList.Add(new TeamScheduleTimeLineViewModelToggle75989Off
				{
					HourText = string.Empty,
					LengthInMinutesToDisplay = lengthInMinutes,
					StartTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.StartDateTime, timeZone),
					EndTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.StartDateTime.AddMinutes(lengthInMinutes), timeZone)
				});
				shiftStartRounded = timeLinePeriod.StartDateTime.AddMinutes(lengthInMinutes);
			}
			if (timeLinePeriod.EndDateTime.Minute != 0)
			{
				shiftEndRounded = timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute);
				lastHour = new TeamScheduleTimeLineViewModelToggle75989Off
				{
					HourText = _createHourText.CreateText(shiftEndRounded),
					LengthInMinutesToDisplay = timeLinePeriod.EndDateTime.Minute,
					StartTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute), timeZone),
					EndTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.EndDateTime, timeZone)
				};
			}

			for (var time = shiftStartRounded; time < shiftEndRounded; time = time.AddHours(1))
			{
				hourList.Add(new TeamScheduleTimeLineViewModelToggle75989Off
				{
					HourText = _createHourText.CreateText(time),
					LengthInMinutesToDisplay = 60,
					StartTime = TimeZoneHelper.ConvertFromUtc(time, timeZone),
					EndTime = TimeZoneHelper.ConvertFromUtc(time.AddMinutes(60), timeZone)
				});
			}

			if (lastHour != null)
				hourList.Add(lastHour);

			return hourList.ToArray();
		}
	}

	public class TimeLineViewModelFactory : ITimeLineViewModelFactory
	{
		private readonly IUserTimeZone _userTimeZone;

		public TimeLineViewModelFactory(IUserTimeZone userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		public TimeLineViewModel[] CreateTimeLineHours(DateTimePeriod timeLinePeriod)
		{
			var hourList = new List<TimeLineViewModel>();
			var date = timeLinePeriod.StartDateTime.Date;
			var startTime = timeLinePeriod.StartDateTime.TimeOfDay;
			var endTime = timeLinePeriod.EndDateTime.TimeOfDay;
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

			for (var hour = firstHour; hour <= lastHour; hour = hour.Add(TimeSpan.FromHours(1)))
			{
				hourList.Add(new TeamScheduleTimeLineViewModel
				{
					Time = TimeZoneHelper.ConvertFromUtc(date.Add(hour), timeZone).TimeOfDay,
					TimeLineDisplay = date.Add(hour).ToLocalizedTimeFormat(),
					PositionPercentage = diff == TimeSpan.Zero ? 0 : Math.Round((decimal)(hour - startTime).Ticks / diff.Ticks, 4)
				});
			}

			return hourList.ToArray();
		}
	}
}