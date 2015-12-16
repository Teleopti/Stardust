using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TimeLineViewModelReworkedFactory : ITimeLineViewModelReworkedFactory
	{
		private readonly ICreateHourText _createHourText;
		private readonly IUserTimeZone _userTimeZone;

		public TimeLineViewModelReworkedFactory(ICreateHourText createHourText, IUserTimeZone userTimeZone)
		{
			_createHourText = createHourText;
			_userTimeZone = userTimeZone;
		}


		public TimeLineViewModelReworked[] CreateTimeLineHours(DateTimePeriod timeLinePeriod)
		{
			var hourList = new List<TimeLineViewModelReworked>();
			TimeLineViewModelReworked lastHour = null;
			var shiftStartRounded = timeLinePeriod.StartDateTime;
			var shiftEndRounded = timeLinePeriod.EndDateTime;
			var timeZone = _userTimeZone.TimeZone();

			if (timeLinePeriod.StartDateTime.Minute != 0)
			{
				var lengthInMinutes = 60 - timeLinePeriod.StartDateTime.Minute;
				hourList.Add(new TimeLineViewModelReworked
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
				lastHour = new TimeLineViewModelReworked
					           {
								   HourText = _createHourText.CreateText(shiftEndRounded),
						           LengthInMinutesToDisplay = timeLinePeriod.EndDateTime.Minute,
								   StartTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute), timeZone),
								   EndTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.EndDateTime, timeZone)
					           };
			}

			for (var time = shiftStartRounded; time < shiftEndRounded; time = time.AddHours(1))
			{
				hourList.Add(new TimeLineViewModelReworked
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