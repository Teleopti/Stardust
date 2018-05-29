﻿using System;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly ITeamSchedulePersonsProvider _teamSchedulePersonsProvider;
		private readonly IAgentScheduleViewModelMapper _agentScheduleViewModelMapper;
		private readonly ITimeLineViewModelMapper _timeLineViewModelMapper;
		private readonly IPersonScheduleDayReadModelFinder _scheduleDayReadModelFinder;
		private readonly IPersonRepository _personRep;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ITeamScheduleProjectionProvider _projectionProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _logonUser;
		private readonly TeamScheduleAgentScheduleViewModelMapper _teamScheduleAgentScheduleViewModelMapper;

		public TeamScheduleViewModelFactory(ITeamSchedulePersonsProvider teamSchedulePersonsProvider,
			IAgentScheduleViewModelMapper agentScheduleViewModelMapper,
			ITimeLineViewModelMapper timeLineViewModelMapper,
			IPersonScheduleDayReadModelFinder scheduleDayReadModelFinder, IPersonRepository personRep
			, IScheduleProvider scheduleProvider, ITeamScheduleProjectionProvider projectionProvider
			, IPermissionProvider permissionProvider, ILoggedOnUser logonUser, TeamScheduleAgentScheduleViewModelMapper teamScheduleAgentScheduleViewModelMapper)
		{
			_teamSchedulePersonsProvider = teamSchedulePersonsProvider;
			_agentScheduleViewModelMapper = agentScheduleViewModelMapper;
			_timeLineViewModelMapper = timeLineViewModelMapper;
			_scheduleDayReadModelFinder = scheduleDayReadModelFinder;
			_personRep = personRep;
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_permissionProvider = permissionProvider;
			_logonUser = logonUser;
			_teamScheduleAgentScheduleViewModelMapper = teamScheduleAgentScheduleViewModelMapper;
		}

		public TeamScheduleViewModel GetTeamScheduleViewModel(TeamScheduleViewModelData data)
		{
			if (data.Paging.Equals(Paging.Empty) || data.Paging.Take <= 0)
			{
				return new TeamScheduleViewModel();
			}

			var currentUser = _logonUser.CurrentUser();
			var isSchedulePublished = _permissionProvider.IsPersonSchedulePublished(data.ScheduleDate, currentUser,
				ScheduleVisibleReasons.Any);
			var canSeeUnpublished =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var myScheduleDay = isSchedulePublished || canSeeUnpublished
				? _scheduleProvider.GetScheduleForPersons(data.ScheduleDate, new[] { currentUser }).SingleOrDefault()
				: null;

			var myScheduleViewModel = _projectionProvider.MakeScheduleReadModel(currentUser, myScheduleDay, true);

			int pageCount;
			List<AgentInTeamScheduleViewModel> agentSchedules;
			if (data.TimeFilter == null && data.TimeSortOrder.IsNullOrEmpty())
			{
				agentSchedules = constructAgentSchedulesWithoutReadModel(data, out pageCount);
			}
			else
			{
				agentSchedules = constructAgentSchedulesFromReadModel(data, false, out pageCount);
				var mySchedule = agentSchedules.SingleOrDefault(x => x.PersonId == currentUser.Id.GetValueOrDefault());
				agentSchedules.Remove(mySchedule);
			}

			var timeLineHours = _timeLineViewModelMapper.Map(agentSchedules.Concat(Enumerable.Repeat(myScheduleViewModel, 1)), data.ScheduleDate).ToArray();
			var timeLineStartTimeSpan =timeLineHours.First().Time.Add(TimeSpan.FromMinutes(-15));
			var timeLineEndTimeSpan = timeLineHours.Last().Time.Add(TimeSpan.FromMinutes(15));
			var timeLineStartTime = data.ScheduleDate.Date.Add(timeLineStartTimeSpan);
			var timeLineEndTime = data.ScheduleDate.Date.Add(timeLineEndTimeSpan);

			return new TeamScheduleViewModel
			{
				MySchedule = _teamScheduleAgentScheduleViewModelMapper
					.Map(new[] {myScheduleViewModel}, timeLineStartTime, timeLineEndTime).FirstOrDefault(),
				AgentSchedules = _teamScheduleAgentScheduleViewModelMapper.Map(agentSchedules, timeLineStartTime, timeLineEndTime)
					.ToArray(),
				TimeLine = timeLineHours,
				PageCount = pageCount
			};
		}
		private List<AgentInTeamScheduleViewModel> constructAgentSchedulesFromReadModel(TeamScheduleViewModelData data, bool isMyScheduleIncluded, out int pageCount)
		{
			var personIds = _teamSchedulePersonsProvider.RetrievePersonIds(data).ToList();
			var currentUser = _logonUser.CurrentUser();
			if (!isMyScheduleIncluded && personIds.Contains(currentUser.Id.GetValueOrDefault()))
			{
				personIds.Remove(currentUser.Id.GetValueOrDefault());
			}

			var personScheduleDays = _scheduleDayReadModelFinder.ForPersons(data.ScheduleDate, personIds, data.Paging,
				data.TimeFilter, data.TimeSortOrder);
			var resultPersonId = personScheduleDays.Select(p => p.PersonId);
			var people = _personRep.FindPeople(resultPersonId).ToLookup(p => p.Id.GetValueOrDefault());
			var schedulesWithPersons = from s in personScheduleDays
									   select new PersonSchedule
									   {
										   Person = people[s.PersonId].FirstOrDefault(),
										   Schedule = s,
										   Date = data.ScheduleDate
									   };

			var agentSchedules = _agentScheduleViewModelMapper.Map(schedulesWithPersons).ToList();
			var scheduleCount = agentSchedules.Any() ? agentSchedules.First().Total : 0;
			pageCount = (int)Math.Ceiling((double)scheduleCount / data.Paging.Take);

			return agentSchedules;
		}

		private List<AgentInTeamScheduleViewModel> constructAgentSchedulesWithoutReadModel(TeamScheduleViewModelData data, out int pageCount)
		{
			var people = _teamSchedulePersonsProvider.RetrievePeople(data).ToList();
			var currentUser = _logonUser.CurrentUser();
			people.Remove(currentUser);

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
					var scheduleReadModel = _projectionProvider.MakeScheduleReadModel(person, scheduleDay, isPermittedToViewConfidential);
					agentSchedules.Add(scheduleReadModel);
				}
			}
			return agentSchedules;
		}
	}
}