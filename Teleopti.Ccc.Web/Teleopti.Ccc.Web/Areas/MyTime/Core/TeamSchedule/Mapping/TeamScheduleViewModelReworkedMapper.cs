using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
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
		private readonly IPersonScheduleDayReadModelFinder _scheduleDayReadModelFinder;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonRepository _personRep;

		public TeamScheduleViewModelReworkedMapper(ITeamSchedulePersonsProvider teamSchedulePersonsProvider, IAgentScheduleViewModelReworkedMapper agentScheduleViewModelReworkedMapper, ITimeLineViewModelReworkedMapper timeLineViewModelReworkedMapper, IPersonScheduleDayReadModelFinder scheduleDayReadModelFinder, IPermissionProvider permissionProvider, IPersonRepository personRep)
		{
			_teamSchedulePersonsProvider = teamSchedulePersonsProvider;
			_agentScheduleViewModelReworkedMapper = agentScheduleViewModelReworkedMapper;
			_timeLineViewModelReworkedMapper = timeLineViewModelReworkedMapper;
			_scheduleDayReadModelFinder = scheduleDayReadModelFinder;
			_permissionProvider = permissionProvider;
			_personRep = personRep;
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
				var people = _personRep.FindPeople(personIds);
				var schedulesWithPersons = from s in personScheduleDays
						   let person = (from p in people
								 where p.Id.Value == s.PersonId
								 select p).SingleOrDefault()
						   where person != null
						   select new PersonSchedule()
						   {
							   Person = person,
							   Schedule = s
						   };

				agentSchedules = _agentScheduleViewModelReworkedMapper.Map(schedulesWithPersons).ToList();
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