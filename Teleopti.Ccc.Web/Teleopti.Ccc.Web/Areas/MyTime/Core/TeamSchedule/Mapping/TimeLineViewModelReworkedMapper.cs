using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TimeLineViewModelReworkedMapper : ITimeLineViewModelReworkedMapper
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly ITimeLineViewModelReworkedFactory _timeLineViewModelReworkedFactory;

		public TimeLineViewModelReworkedMapper(ITimeLineViewModelReworkedFactory timeLineViewModelReworkedFactory, IUserTimeZone userTimeZone)
		{
			_timeLineViewModelReworkedFactory = timeLineViewModelReworkedFactory;
			_userTimeZone = userTimeZone;
		}

		public TimeLineViewModelReworked[] Map(IEnumerable<AgentScheduleViewModelReworked>
			                                                         agentSchedules, DateOnly date)
		{
			return _timeLineViewModelReworkedFactory.CreateTimeLineHours(getTimeLinePeriod(agentSchedules, date));
		}

		private DateTimePeriod getTimeLinePeriod(IEnumerable<AgentScheduleViewModelReworked>
			agentSchedules, DateOnly date)
		{
			DateTimePeriod? scheduleMinMaxPeriod = getScheduleMinMax(agentSchedules);

			var timeZone = _userTimeZone.TimeZone();

			var returnPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date.Date.AddHours(8),
				date.Date.AddHours(17), timeZone);

			if (scheduleMinMaxPeriod.HasValue)
				returnPeriod = scheduleMinMaxPeriod.Value;

			returnPeriod = returnPeriod.ChangeStartTime(new TimeSpan(0, -15, 0));
			returnPeriod = returnPeriod.ChangeEndTime(new TimeSpan(0, 15, 0));
			return returnPeriod;
		}

		private DateTimePeriod? getScheduleMinMax(IEnumerable<AgentScheduleViewModelReworked> agentSchedules)
		{
			var schedules = (agentSchedules as IList<AgentScheduleViewModelReworked>) ?? agentSchedules.ToList();

			var schedulesWithoutDayoffAndEmptyDays = schedules.Where(s => s.IsDayOff == false && (!s.ScheduleLayers.IsNullOrEmpty())).ToList();

			if (!schedulesWithoutDayoffAndEmptyDays.Any())
				return null;

			var timeZone = _userTimeZone.TimeZone();

			var startTime = schedulesWithoutDayoffAndEmptyDays.Min(s => s.ScheduleLayers.First().Start);
			var endTime = schedulesWithoutDayoffAndEmptyDays.Max(l => l.ScheduleLayers.Last().End);

			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startTime, endTime, timeZone);
		}
	}
}