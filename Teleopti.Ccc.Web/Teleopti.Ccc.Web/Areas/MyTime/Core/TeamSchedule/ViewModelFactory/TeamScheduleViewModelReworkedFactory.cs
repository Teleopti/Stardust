using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamScheduleViewModelReworkedFactory : ITeamScheduleViewModelReworkedFactory
	{
		private readonly ITeamSchedulePersonsProvider _teamSchedulePersonsProvider;
		private readonly IAgentScheduleViewModelReworkedMapper _agentScheduleViewModelReworkedMapper;
		private readonly ITimeLineViewModelReworkedMapper _timeLineViewModelReworkedMapper;
		private readonly IPersonScheduleDayReadModelFinder _scheduleDayReadModelFinder;
		private readonly IPersonRepository _personRep;
		private IScheduleProvider _scheduleProvider;
		private ITeamScheduleProjectionProvider _projectionProvider;
		private IPermissionProvider _permissionProvider;
		private ILoggedOnUser _LogonUser;

		public TeamScheduleViewModelReworkedFactory(ITeamSchedulePersonsProvider teamSchedulePersonsProvider,
			IAgentScheduleViewModelReworkedMapper agentScheduleViewModelReworkedMapper,
			ITimeLineViewModelReworkedMapper timeLineViewModelReworkedMapper,
			IPersonScheduleDayReadModelFinder scheduleDayReadModelFinder, IPersonRepository personRep, IScheduleProvider scheduleProvider, ITeamScheduleProjectionProvider projectionProvider, IPermissionProvider permissionProvider, ILoggedOnUser logonUser)
		{
			_teamSchedulePersonsProvider = teamSchedulePersonsProvider;
			_agentScheduleViewModelReworkedMapper = agentScheduleViewModelReworkedMapper;
			_timeLineViewModelReworkedMapper = timeLineViewModelReworkedMapper;
			_scheduleDayReadModelFinder = scheduleDayReadModelFinder;
			_personRep = personRep;
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_permissionProvider = permissionProvider;
			_LogonUser = logonUser;
		}

		public TeamScheduleViewModelReworked GetViewModel(TeamScheduleViewModelData data)
		{
			if (data.Paging == null || data.Paging.Take <= 0)
			{
				return new TeamScheduleViewModelReworked();
			}

			var personIds = _teamSchedulePersonsProvider.RetrievePersonIds(data).ToList();

			AgentScheduleViewModelReworked[] agentSchedules;

			int pageCount = 1;
			if (personIds.Any())
			{
				var personScheduleDays = _scheduleDayReadModelFinder.ForPersons(data.ScheduleDate, personIds, data.Paging,
					data.TimeFilter, data.TimeSortOrder);
				var resultPersonId = personScheduleDays.Select(p=>p.PersonId);
				var people = _personRep.FindPeople(resultPersonId);
				var schedulesWithPersons = from s in personScheduleDays
					let person = (from p in people
						where p.Id.Value == s.PersonId
						select p).SingleOrDefault()
					where person != null
					select new PersonSchedule()
					{
						Person = person,
						Schedule = s,
						Date = data.ScheduleDate
					};

				agentSchedules = _agentScheduleViewModelReworkedMapper.Map(schedulesWithPersons).ToArray();
				int scheduleCount = agentSchedules.Any() ? agentSchedules.First().Total : 0;
				pageCount = (int) Math.Ceiling(((double) scheduleCount)/data.Paging.Take);
			}
			else
			{
				agentSchedules = new AgentScheduleViewModelReworked[] {};
			}

			var timeLineHours = _timeLineViewModelReworkedMapper.Map(agentSchedules, data.ScheduleDate);
		
			return new TeamScheduleViewModelReworked
			{
				AgentSchedules = agentSchedules,
				TimeLine = timeLineHours,
				PageCount = pageCount
			};
		}

		public TeamScheduleViewModelReworked GetViewModelNoReadModel(TeamScheduleViewModelData data)
		{
			if (data.Paging == null || data.Paging.Take <= 0)
			{
				return new TeamScheduleViewModelReworked();
			}

			var myScheduleDay = _permissionProvider.IsPersonSchedulePublished(data.ScheduleDate, _LogonUser.CurrentUser(),
				ScheduleVisibleReasons.Any)
				? _scheduleProvider.GetScheduleForPersons(data.ScheduleDate, new[] {_LogonUser.CurrentUser()}).SingleOrDefault()
				: null;
		
			var myScheduleViewModel = _projectionProvider.MakeScheduleReadModel(_LogonUser.CurrentUser(), myScheduleDay, true);
			
			
			var people = _teamSchedulePersonsProvider.RetrievePeople(data).ToList();
			if (people.Contains(_LogonUser.CurrentUser()))
			{
				people.Remove(_LogonUser.CurrentUser());
			}
			var isPermittedToViewConfidential = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential);
			var agentSchedules = new List<AgentScheduleViewModelReworked>();
			int pageCount = 1;
			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			if (people.Any())
			{
				var scheduleDays = _scheduleProvider.GetScheduleForPersons(data.ScheduleDate, people).ToList();

				var personScheduleDays = (from p in people
										  let personSchedule = (from s in scheduleDays where s.Person == p select s).SingleOrDefault()
										  select new Tuple<IPerson, IScheduleDay>(p, personSchedule)).ToArray();
				var sortedScheduleDays = personScheduleDays.OrderBy(personScheduleDay =>
				{
					var person = personScheduleDay.Item1;
					var schedule = personScheduleDay.Item2;
					var isPublished = _permissionProvider.IsPersonSchedulePublished(data.ScheduleDate,
						person, ScheduleVisibleReasons.Any);
					var sortValue = TeamScheduleSortingUtil.GetSortedValue(schedule, canSeeUnpublishedSchedules, isPublished);
					return sortValue;
				}).ThenBy(personScheduleDay => personScheduleDay.Item1.Name.LastName);

				var requestedScheduleDays = sortedScheduleDays.Skip(data.Paging.Skip).Take(data.Paging.Take);
				foreach (var personScheduleDay in requestedScheduleDays)
				{
					var person = personScheduleDay.Item1;
					var scheduleDay = _permissionProvider.IsPersonSchedulePublished(data.ScheduleDate,
						person, ScheduleVisibleReasons.Any) ? personScheduleDay.Item2:null;
					var scheduleReadModel = _projectionProvider.MakeScheduleReadModel(person, scheduleDay, isPermittedToViewConfidential);
					agentSchedules.Add(scheduleReadModel);
				}
			}

			var timeLineHours = _timeLineViewModelReworkedMapper.Map(agentSchedules.Union(new[]{myScheduleViewModel}), data.ScheduleDate).ToArray();

			return new TeamScheduleViewModelReworked
			{
				MySchedule = myScheduleViewModel,
				AgentSchedules = agentSchedules.ToArray(),
				TimeLine = timeLineHours,
				PageCount = pageCount
			};
		}
	}
}