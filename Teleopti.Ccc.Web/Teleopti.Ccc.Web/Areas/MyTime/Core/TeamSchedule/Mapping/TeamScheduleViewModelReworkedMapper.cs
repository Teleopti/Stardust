using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public class TeamScheduleViewModelReworkedMapper : ITeamScheduleViewModelReworkedMapper
	{
		private readonly ITeamSchedulePersonsProvider _teamSchedulePersonsProvider;
		private readonly IAgentScheduleViewModelReworkedMapper _agentScheduleViewModelReworkedMapper;
		private readonly ITimeLineViewModelReworkedMapper _timeLineViewModelReworkedMapper;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly IPersonScheduleDayReadModelFinder _scheduleDayReadModelFinder;
		private readonly ILoggedOnUser _loggedOnUser;

		public TeamScheduleViewModelReworkedMapper(ITeamSchedulePersonsProvider teamSchedulePersonsProvider, IAgentScheduleViewModelReworkedMapper agentScheduleViewModelReworkedMapper, ITimeLineViewModelReworkedMapper timeLineViewModelReworkedMapper, IScheduleProvider scheduleProvider, ILoggedOnUser loggedOnUser, IPersonScheduleDayReadModelFinder scheduleDayReadModelFinder)
		{
			_teamSchedulePersonsProvider = teamSchedulePersonsProvider;
			_agentScheduleViewModelReworkedMapper = agentScheduleViewModelReworkedMapper;
			_timeLineViewModelReworkedMapper = timeLineViewModelReworkedMapper;
			_scheduleProvider = scheduleProvider;
			_loggedOnUser = loggedOnUser;
			_scheduleDayReadModelFinder = scheduleDayReadModelFinder;
		}

		public TeamScheduleViewModelReworked Map(TeamScheduleViewModelData data)
		{
			if (data.Paging == null || data.Paging.Take <= 0)
			{
				return new TeamScheduleViewModelReworked();
			}

			var personIds = _teamSchedulePersonsProvider.RetrievePersons(data).ToList();

			IEnumerable<AgentScheduleViewModelReworked> agentSchedules;
			int pageCount = 1;
			if (personIds.Any())
			{
				var personScheduleDays = _scheduleDayReadModelFinder.ForPersons(data.ScheduleDate, personIds, data.Paging,
					data.TimeFilter, data.TimeSortOrder);

				agentSchedules = _agentScheduleViewModelReworkedMapper.Map(personScheduleDays).ToList();
				int scheduleCount = agentSchedules.Any() ? agentSchedules.First().Total : 0;
				pageCount = (int) Math.Ceiling(((double) scheduleCount)/data.Paging.Take);
			}
			else
			{
				agentSchedules = new List<AgentScheduleViewModelReworked>();				
			}

			var timeLineHours = _timeLineViewModelReworkedMapper.Map(agentSchedules, data.ScheduleDate);

			return new TeamScheduleViewModelReworked
			{
				AgentSchedules = agentSchedules,
				TimeLine = timeLineHours,
				PageCount = pageCount
			};
		}
	}
}