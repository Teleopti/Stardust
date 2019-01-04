using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamScheduleViewModelFactoryToggle75989Off : ITeamScheduleViewModelFactoryToggle75989Off
	{
		private readonly ITeamSchedulePersonsProvider _teamSchedulePersonsProvider;
		private readonly ITimeLineViewModelMapperToggle75989Off _timeLineViewModelMapper;
		private readonly IPersonScheduleDayReadModelFinder _scheduleDayReadModelFinder;
		private readonly IPersonRepository _personRep;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly TeamScheduleShiftViewModelProvider _shiftViewModelProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _logonUser;

		public TeamScheduleViewModelFactoryToggle75989Off(ITeamSchedulePersonsProvider teamSchedulePersonsProvider,
			ITimeLineViewModelMapperToggle75989Off timeLineViewModelMapper,
			IPersonScheduleDayReadModelFinder scheduleDayReadModelFinder, IPersonRepository personRep, IScheduleProvider scheduleProvider, 
			TeamScheduleShiftViewModelProvider shiftViewModelProvider, IPermissionProvider permissionProvider, ILoggedOnUser logonUser)
		{
			_teamSchedulePersonsProvider = teamSchedulePersonsProvider;
			_timeLineViewModelMapper = timeLineViewModelMapper;
			_scheduleDayReadModelFinder = scheduleDayReadModelFinder;
			_personRep = personRep;
			_scheduleProvider = scheduleProvider;
			_shiftViewModelProvider = shiftViewModelProvider;
			_permissionProvider = permissionProvider;
			_logonUser = logonUser;
		}

		public TeamScheduleViewModelToggle75989Off GetTeamScheduleViewModel(TeamScheduleViewModelData data)
		{
			if (data.Paging.Equals(Paging.Empty) || data.Paging.Take <= 0)
			{
				return new TeamScheduleViewModelToggle75989Off();
			}

			var currentUser = _logonUser.CurrentUser();
			var isSchedulePublished = _permissionProvider.IsPersonSchedulePublished(data.ScheduleDate, currentUser,
				ScheduleVisibleReasons.Any);
			var canSeeUnpublished =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var myScheduleDay = isSchedulePublished || canSeeUnpublished
				? _scheduleProvider.GetScheduleForPersons(data.ScheduleDate, new[] { currentUser }).SingleOrDefault()
				: null;

			var myScheduleViewModel = _shiftViewModelProvider.MakeScheduleReadModel(currentUser, currentUser, myScheduleDay, true);

			int pageCount;
			var agentSchedules = constructAgentSchedules(data, out pageCount);

			var timeLineHours = _timeLineViewModelMapper.Map(agentSchedules.Concat(Enumerable.Repeat(myScheduleViewModel, 1)), data.ScheduleDate).ToArray();

			return new TeamScheduleViewModelToggle75989Off
			{
				MySchedule = myScheduleViewModel,
				AgentSchedules = agentSchedules.ToArray(),
				TimeLine = timeLineHours,
				PageCount = pageCount
			};
		}

		private List<AgentInTeamScheduleViewModel> constructAgentSchedules(TeamScheduleViewModelData data, out int pageCount)
		{
			var currentUser = _logonUser.CurrentUser();
			List<IPerson> people;
			if (data.TimeFilter == null && data.TimeSortOrder.IsNullOrEmpty())
			{
				people = _teamSchedulePersonsProvider.RetrievePeople(data).ToList();
				people.Remove(currentUser);
			}
			else
			{
				var personIds = _teamSchedulePersonsProvider.RetrievePersonIds(data).ToList();
				personIds.Remove(currentUser.Id.GetValueOrDefault());

				var personScheduleDays = _scheduleDayReadModelFinder.ForPersons(data.ScheduleDate, personIds, data.Paging, data.TimeFilter, data.TimeSortOrder);
				var resultPersonId = personScheduleDays.Select(p => p.PersonId);
				people = _personRep.FindPeople(resultPersonId).ToList();
			}

			return getAgentSchedules(currentUser, data, people, out pageCount);
		}

		private List<AgentInTeamScheduleViewModel> getAgentSchedules(IPerson currentUser, TeamScheduleViewModelData data, IList<IPerson> people, out int pageCount)
		{
			var isPermittedToViewConfidential =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential);
			var agentSchedules = new List<AgentInTeamScheduleViewModel>();
			pageCount = 1;
			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			if (people.Any())
			{
				pageCount = (int)Math.Ceiling((double)people.Count / data.Paging.Take);
				var scheduleDays = _scheduleProvider.GetScheduleForPersons(data.ScheduleDate, people).ToLookup(s => s.Person);

				var personScheduleDays = people.Select(p => new Tuple<IPerson, IScheduleDay>(p, scheduleDays[p].FirstOrDefault())).ToArray();
				Array.Sort(personScheduleDays, new TeamScheduleComparer(canSeeUnpublishedSchedules, _permissionProvider, false) { ScheduleVisibleReason = ScheduleVisibleReasons.Any });

				var requestedScheduleDays = personScheduleDays.Skip(data.Paging.Skip).Take(data.Paging.Take);
				foreach (var personScheduleDay in requestedScheduleDays)
				{
					var person = personScheduleDay.Item1;
					var scheduleDay = _permissionProvider.IsPersonSchedulePublished(data.ScheduleDate,
										  person, ScheduleVisibleReasons.Any) || canSeeUnpublishedSchedules
						? personScheduleDay.Item2
						: null;
					var scheduleReadModel = _shiftViewModelProvider.MakeScheduleReadModel(currentUser, person, scheduleDay, isPermittedToViewConfidential);
					agentSchedules.Add(scheduleReadModel);
				}
			}
			return agentSchedules;
		}
	}
}