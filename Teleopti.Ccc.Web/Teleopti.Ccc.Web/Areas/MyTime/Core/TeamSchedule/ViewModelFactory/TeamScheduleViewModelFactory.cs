using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
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
		private readonly IPersonScheduleDayReadModelFinder _scheduleDayReadModelFinder;
		private readonly IPersonRepository _personRep;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ITeamScheduleProjectionProvider _projectionProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _logonUser;
		private readonly TeamScheduleAgentScheduleViewModelMapper _teamScheduleAgentScheduleViewModelMapper;
		private readonly ITimeLineViewModelFactory _timeLineViewModelFactory;

		public TeamScheduleViewModelFactory(ITeamSchedulePersonsProvider teamSchedulePersonsProvider,
			IAgentScheduleViewModelMapper agentScheduleViewModelMapper,
			IPersonScheduleDayReadModelFinder scheduleDayReadModelFinder, IPersonRepository personRep
			, IScheduleProvider scheduleProvider, ITeamScheduleProjectionProvider projectionProvider
			, IPermissionProvider permissionProvider, ILoggedOnUser logonUser
			, TeamScheduleAgentScheduleViewModelMapper teamScheduleAgentScheduleViewModelMapper
			, ITimeLineViewModelFactory timeLineViewModelFactory)
		{
			_teamSchedulePersonsProvider = teamSchedulePersonsProvider;
			_agentScheduleViewModelMapper = agentScheduleViewModelMapper;
			_scheduleDayReadModelFinder = scheduleDayReadModelFinder;
			_personRep = personRep;
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_permissionProvider = permissionProvider;
			_logonUser = logonUser;
			_teamScheduleAgentScheduleViewModelMapper = teamScheduleAgentScheduleViewModelMapper;
			_timeLineViewModelFactory = timeLineViewModelFactory;
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
			int totalAgentCount;
			List<AgentInTeamScheduleViewModel> agentSchedules;
			if (data.TimeFilter == null && data.TimeSortOrder.IsNullOrEmpty())
			{
				var result = constructAgentSchedulesWithoutReadModel(data);
				agentSchedules = result.Item1;
				pageCount = result.Item2;
				totalAgentCount = result.Item3;
			}
			else
			{
				var result = constructAgentSchedulesFromReadModel(data, false);
				agentSchedules = result.Item1;
				pageCount = result.Item2;
				totalAgentCount = result.Item3;
				var mySchedule = agentSchedules.SingleOrDefault(x => x.PersonId == currentUser.Id.GetValueOrDefault());
				agentSchedules.Remove(mySchedule);
			}

			var schedulePeriod =
				getSchedulePeriod(agentSchedules.Concat(Enumerable.Repeat(myScheduleViewModel, 1)), data.ScheduleDate);

			var timeLineHours = _timeLineViewModelFactory.CreateTimeLineHours(schedulePeriod);
			var timezone = currentUser.PermissionInformation.DefaultTimeZone();

			return new TeamScheduleViewModel
			{
				MySchedule = _teamScheduleAgentScheduleViewModelMapper
					.Map(new[] {myScheduleViewModel}, schedulePeriod, timezone).FirstOrDefault(),
				AgentSchedules = _teamScheduleAgentScheduleViewModelMapper.Map(agentSchedules, schedulePeriod, timezone)
					.ToArray(),
				TimeLine = timeLineHours,
				PageCount = pageCount,
				TotalAgentCount = totalAgentCount
			};
		}
		private Tuple<List<AgentInTeamScheduleViewModel>, int, int> constructAgentSchedulesFromReadModel(TeamScheduleViewModelData data, bool isMyScheduleIncluded)
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
			var pageCount = (int)Math.Ceiling((double)scheduleCount / data.Paging.Take);

			return new Tuple<List<AgentInTeamScheduleViewModel>, int, int>(agentSchedules, pageCount, scheduleCount);
		}

		private Tuple<List<AgentInTeamScheduleViewModel>, int, int> constructAgentSchedulesWithoutReadModel(TeamScheduleViewModelData data)
		{
			var people = _teamSchedulePersonsProvider.RetrievePeople(data).ToList();
			var currentUser = _logonUser.CurrentUser();
			people.Remove(currentUser);

			var isPermittedToViewConfidential =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewConfidential);
			var agentSchedules = new List<AgentInTeamScheduleViewModel>();
			var pageCount = 1;
			var totalAgentCount = people.Count;
			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			if (people.Any())
			{
				pageCount = (int)Math.Ceiling((double)totalAgentCount / data.Paging.Take);
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
			return new Tuple<List<AgentInTeamScheduleViewModel>, int, int>(agentSchedules, pageCount, totalAgentCount);
		}

		private DateTimePeriod getSchedulePeriod(IEnumerable<AgentInTeamScheduleViewModel>
			agentSchedules, DateOnly date)
		{
			DateTimePeriod? scheduleMinMaxPeriod = getScheduleMinMax(agentSchedules);

			var timeZone = _logonUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var returnPeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date.Date.AddHours(DefaultSchedulePeriodProvider.DefaultStartHour),
				date.Date.AddHours(DefaultSchedulePeriodProvider.DefaultEndHour), timeZone);

			if (scheduleMinMaxPeriod.HasValue)
			{
				var startDateTime = scheduleMinMaxPeriod.Value.StartDateTime;
				if (returnPeriod.StartDateTime < startDateTime)
				{
					startDateTime = returnPeriod.StartDateTime;
				}

				var endDateTime = scheduleMinMaxPeriod.Value.EndDateTime;
				if (returnPeriod.EndDateTime > endDateTime)
				{
					endDateTime = returnPeriod.EndDateTime;
				}

				returnPeriod = new DateTimePeriod(startDateTime, endDateTime);
			}

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

			var timeZone = _logonUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var startTime = schedulesWithoutEmptyLayerDays.Min(s => s.ScheduleLayers.First().Start);
			var endTime = schedulesWithoutEmptyLayerDays.Max(l => l.ScheduleLayers.Last().End);

			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startTime, endTime, timeZone);
		}
	}
}