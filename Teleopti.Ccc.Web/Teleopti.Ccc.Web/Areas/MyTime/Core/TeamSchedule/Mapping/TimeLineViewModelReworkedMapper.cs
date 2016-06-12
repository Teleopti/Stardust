﻿using System;
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

		public TimeLineViewModelReworked[] Map(IEnumerable<AgentInTeamScheduleViewModel>
			                                                         agentSchedules, DateOnly date)
		{
			return _timeLineViewModelReworkedFactory.CreateTimeLineHours(getTimeLinePeriod(agentSchedules, date));
		}

		private DateTimePeriod getTimeLinePeriod(IEnumerable<AgentInTeamScheduleViewModel>
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

		private DateTimePeriod? getScheduleMinMax(IEnumerable<AgentInTeamScheduleViewModel> agentSchedules)
		{
			var schedules = (agentSchedules as IList<AgentInTeamScheduleViewModel>) ?? agentSchedules.ToList();

			var schedulesWithoutEmptyLayerDays = schedules.Where(s => (!s.ScheduleLayers.IsNullOrEmpty())).ToList();

			if (!schedulesWithoutEmptyLayerDays.Any())
				return null;

			var timeZone = _userTimeZone.TimeZone();

			var startTime = schedulesWithoutEmptyLayerDays.Min(s => s.ScheduleLayers.First().Start);
			var endTime = schedulesWithoutEmptyLayerDays.Max(l => l.ScheduleLayers.Last().End);

			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startTime, endTime, timeZone);
		}
	}
}