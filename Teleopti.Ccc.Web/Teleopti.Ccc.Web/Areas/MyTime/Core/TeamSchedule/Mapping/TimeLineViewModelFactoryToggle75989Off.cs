using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TimeLineViewModelFactoryToggle75989Off : ITimeLineViewModelFactoryToggle75989Off
	{
		private readonly ICreateHourText _createHourText;
		private readonly IUserTimeZone _userTimeZone;

		public TimeLineViewModelFactoryToggle75989Off(ICreateHourText createHourText, IUserTimeZone userTimeZone)
		{
			_createHourText = createHourText;
			_userTimeZone = userTimeZone;
		}

		public TeamScheduleTimeLineViewModelToggle75989Off[] CreateTimeLineHours(DateTimePeriod timeLinePeriod)
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
}