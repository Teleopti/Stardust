using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TimeLineViewModelMapperToggle75989Off : ITimeLineViewModelMapperToggle75989Off
	{
		private readonly IUserTimeZone _userTimeZone;
		private readonly ITimeLineViewModelFactoryToggle75989Off _timeLineViewModelFactory;

		public TimeLineViewModelMapperToggle75989Off(ITimeLineViewModelFactoryToggle75989Off timeLineViewModelFactory, IUserTimeZone userTimeZone)
		{
			_timeLineViewModelFactory = timeLineViewModelFactory;
			_userTimeZone = userTimeZone;
		}

		public TeamScheduleTimeLineViewModelToggle75989Off[] Map(IEnumerable<AgentInTeamScheduleViewModel> agentSchedules, DateOnly date)
		{
			return _timeLineViewModelFactory.CreateTimeLineHours(getTimeLinePeriod(agentSchedules, date));
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