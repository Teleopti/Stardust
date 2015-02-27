using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TimeLineViewModelReworkedMapper : ITimeLineViewModelReworkedMapper
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ITimeLineViewModelReworkedFactory _timeLineViewModelReworkedFactory;

		public TimeLineViewModelReworkedMapper(ILoggedOnUser loggedOnUser, ITimeLineViewModelReworkedFactory timeLineViewModelReworkedFactory)
		{
			_loggedOnUser = loggedOnUser;
			_timeLineViewModelReworkedFactory = timeLineViewModelReworkedFactory;
		}

		public IEnumerable<TimeLineViewModelReworked> Map(IEnumerable<AgentScheduleViewModelReworked>
			                                                         agentSchedules, DateOnly date)
		{
			return _timeLineViewModelReworkedFactory.CreateTimeLineHours(getTimeLinePeriod(agentSchedules, date));
		}

		private DateTimePeriod getTimeLinePeriod(IEnumerable<AgentScheduleViewModelReworked>
			agentSchedules, DateOnly date)
		{
			DateTimePeriod? possibleTradeScheduleMinMax = getScheduleMinMax(agentSchedules);

			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var returnPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date.Date.AddHours(8),
				date.Date.AddHours(17), timeZone);

			if (possibleTradeScheduleMinMax.HasValue)
				returnPeriod = possibleTradeScheduleMinMax.Value;

			returnPeriod = returnPeriod.ChangeStartTime(new TimeSpan(0, -15, 0));
			returnPeriod = returnPeriod.ChangeEndTime(new TimeSpan(0, 15, 0));
			return returnPeriod;
		}

		private DateTimePeriod? getScheduleMinMax(IEnumerable<AgentScheduleViewModelReworked> agentSchedules)
		{
			var schedules = (agentSchedules as IList<AgentScheduleViewModelReworked>) ?? agentSchedules.ToList();

			var schedulesWithoutDayoffAndEmptyDays = schedules.Where(s => s.IsDayOff == false && s.ScheduleLayers != null).ToList();

			if (!schedulesWithoutDayoffAndEmptyDays.Any())
				return null;
			
			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var startTime = schedulesWithoutDayoffAndEmptyDays.Min(s => s.ScheduleLayers.First().Start);
			var endTime = schedulesWithoutDayoffAndEmptyDays.Max(l => l.ScheduleLayers.Last().End);

			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startTime, endTime, timeZone);
		}
	}
}