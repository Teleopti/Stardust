using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly ITeamSchedulePersonsProvider _teamSchedulePersonsProvider;
		private readonly IScheduleProvider _scheduleProvider;
		private readonly ITeamScheduleShiftViewModelProvider _shiftViewModelProvider;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _logonUser;
		private readonly TeamScheduleAgentScheduleViewModelMapper _teamScheduleAgentScheduleViewModelMapper;
		private readonly ITimeLineViewModelFactory _timeLineViewModelFactory;

		public TeamScheduleViewModelFactory(ITeamSchedulePersonsProvider teamSchedulePersonsProvider,
			IScheduleProvider scheduleProvider,
			ITeamScheduleShiftViewModelProvider shiftViewModelProvider,
			IPermissionProvider permissionProvider,
			ILoggedOnUser logonUser,
			TeamScheduleAgentScheduleViewModelMapper teamScheduleAgentScheduleViewModelMapper,
			ITimeLineViewModelFactory timeLineViewModelFactory)
		{
			_teamSchedulePersonsProvider = teamSchedulePersonsProvider;
			_scheduleProvider = scheduleProvider;
			_shiftViewModelProvider = shiftViewModelProvider;
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
			var isSchedulePublished = _permissionProvider.IsPersonSchedulePublished(data.ScheduleDate, currentUser, ScheduleVisibleReasons.Any);
			var canSeeUnpublished =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);

			var myScheduleDay = isSchedulePublished || canSeeUnpublished
				? _scheduleProvider.GetScheduleForPersons(data.ScheduleDate, new[] { currentUser }).SingleOrDefault()
				: null;

			var myScheduleViewModel = _shiftViewModelProvider.MakeScheduleReadModel(currentUser, myScheduleDay, true);

			var agentScheduleTupleList = constructAgentSchedulesWithoutReadModel(data);
			var agentSchedules = agentScheduleTupleList.Item1;
			var pageCount = agentScheduleTupleList.Item2;
			var agentCount = agentScheduleTupleList.Item3;

			var schedulePeriodInUtc = getSchedulePeriod(agentSchedules.Concat(Enumerable.Repeat(myScheduleViewModel, 1)), data.ScheduleDate);

			var timeLineHours = _timeLineViewModelFactory.CreateTimeLineHours(schedulePeriodInUtc, data.ScheduleDate);
			var timezone = currentUser.PermissionInformation.DefaultTimeZone();

			return new TeamScheduleViewModel
			{
				MySchedule = _teamScheduleAgentScheduleViewModelMapper
					.Map(new[] { myScheduleViewModel }, schedulePeriodInUtc, timezone).FirstOrDefault(),
				AgentSchedules = _teamScheduleAgentScheduleViewModelMapper.Map(agentSchedules, schedulePeriodInUtc, timezone)
					.ToArray(),
				TimeLine = timeLineHours,
				PageCount = pageCount,
				TotalAgentCount = agentCount
			};
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
			var totalAgentCount = 0;
			var canSeeUnpublishedSchedules =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules);
			if (people.Any())
			{
				var scheduleDays = _scheduleProvider.GetScheduleForPersons(data.ScheduleDate, people).ToLookup(s => s.Person);
				var personScheduleDays = people.Select(p => new Tuple<IPerson, IScheduleDay>(p, scheduleDays[p].FirstOrDefault())).ToArray();
				personScheduleDays = filterSchedules(personScheduleDays, data).ToArray();

				Array.Sort(personScheduleDays, new TeamScheduleComparer(canSeeUnpublishedSchedules, _permissionProvider, false) { ScheduleVisibleReason = ScheduleVisibleReasons.Any });

				totalAgentCount = personScheduleDays.Length;
				pageCount = (int)Math.Ceiling((double)totalAgentCount / data.Paging.Take);

				var requestedScheduleDays = personScheduleDays.Skip(data.Paging.Skip).Take(data.Paging.Take);
				foreach (var personScheduleDay in requestedScheduleDays)
				{
					var person = personScheduleDay.Item1;
					var scheduleDay = _permissionProvider.IsPersonSchedulePublished(data.ScheduleDate,
						person, ScheduleVisibleReasons.Any) || canSeeUnpublishedSchedules
						? personScheduleDay.Item2
						: null;
					var scheduleReadModel = _shiftViewModelProvider.MakeScheduleReadModel(person, scheduleDay, isPermittedToViewConfidential);
					agentSchedules.Add(scheduleReadModel);
				}
			}
			return new Tuple<List<AgentInTeamScheduleViewModel>, int, int>(agentSchedules, pageCount, totalAgentCount);
		}

		private IEnumerable<Tuple<IPerson, IScheduleDay>> filterSchedules(
			IEnumerable<Tuple<IPerson, IScheduleDay>> personScheduleDays, TeamScheduleViewModelData data)
		{
			if (data.TimeFilter == null)
			{
				return personScheduleDays;
			}

			if (data.TimeFilter.IsDayOff)
			{
				return personScheduleDays.Where(a => a.Item2.HasDayOff() == data.TimeFilter.IsDayOff);
			}

			if (data.TimeFilter.OnlyNightShift)
			{
				var timeZone = _logonUser.CurrentUser().PermissionInformation.DefaultTimeZone();
				return personScheduleDays.Where(p =>
				{
					var projections = p.Item2.ProjectionService().CreateProjection();
					if (projections == null || !projections.Any()) return false;

					var startDateTime = projections.First().Period.StartDateTime;
					var endDateTime = projections.Last().Period.EndDateTime;
					var start = TimeZoneHelper.ConvertFromUtc(startDateTime, timeZone);
					var end = TimeZoneHelper.ConvertFromUtc(endDateTime, timeZone);
					return end.Date > start.Date && end.TimeOfDay > TimeSpan.FromMinutes(0);
				});
			}

			DateTime? startTimeStart, startTimeEnd, endTimeStart, endTimeEnd;
			if (!data.TimeFilter.StartTimes.IsNullOrEmpty() && !data.TimeFilter.EndTimes.IsNullOrEmpty())
			{
				startTimeStart = data.TimeFilter.StartTimes.ElementAt(0).StartDateTime;
				startTimeEnd = data.TimeFilter.StartTimes.ElementAt(0).EndDateTime;

				endTimeStart = data.TimeFilter.EndTimes.ElementAt(0).StartDateTime;
				endTimeEnd = data.TimeFilter.EndTimes.ElementAt(0).EndDateTime;

				personScheduleDays = personScheduleDays.Where(p =>
					{
						var projections = p.Item2.ProjectionService().CreateProjection();
						if (projections == null || !projections.Any()) return false;

						var start = projections.First().Period.StartDateTime;
						var end = projections.Last().Period.EndDateTime;
						return startTimeStart <= start && start <= startTimeEnd && endTimeStart <= end && end <= endTimeEnd;
					}
				);
			}
			else if (!data.TimeFilter.StartTimes.IsNullOrEmpty())
			{
				startTimeStart = data.TimeFilter.StartTimes.ElementAt(0).StartDateTime;
				startTimeEnd = data.TimeFilter.StartTimes.ElementAt(0).EndDateTime;
				personScheduleDays = personScheduleDays.Where(p =>
					{
						var projections = p.Item2.ProjectionService().CreateProjection();
						if (projections == null || !projections.Any()) return false;

						var start = projections.First().Period.StartDateTime;
						return startTimeStart <= start && start <= startTimeEnd;
					}
				);
			}
			else if (!data.TimeFilter.EndTimes.IsNullOrEmpty())
			{
				endTimeStart = data.TimeFilter.EndTimes.ElementAt(0).StartDateTime;
				endTimeEnd = data.TimeFilter.EndTimes.ElementAt(0).EndDateTime;
				personScheduleDays = personScheduleDays.Where(p =>
				{
					var projections = p.Item2.ProjectionService().CreateProjection();
					if (projections == null || !projections.Any()) return false;

					var end = projections.Last().Period.EndDateTime;
					return endTimeStart <= end && end <= endTimeEnd;
				});
			}
			return personScheduleDays;
		}

		private DateTimePeriod getSchedulePeriod(IEnumerable<AgentInTeamScheduleViewModel>
			agentSchedules, DateOnly date)
		{
			var scheduleMinMaxPeriod = getScheduleMinMax(agentSchedules);

			var timeZone = _logonUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var returnPeriodInUtc = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date.Date.AddHours(DefaultSchedulePeriodProvider.DefaultStartHour),
				date.Date.AddHours(DefaultSchedulePeriodProvider.DefaultEndHour), timeZone);

			if (scheduleMinMaxPeriod.HasValue)
			{
				var startDateTime = scheduleMinMaxPeriod.Value.StartDateTime;
				if (returnPeriodInUtc.StartDateTime < startDateTime)
				{
					startDateTime = returnPeriodInUtc.StartDateTime;
				}

				var endDateTime = scheduleMinMaxPeriod.Value.EndDateTime;
				if (returnPeriodInUtc.EndDateTime > endDateTime)
				{
					endDateTime = returnPeriodInUtc.EndDateTime;
				}

				returnPeriodInUtc = new DateTimePeriod(startDateTime, endDateTime);
			}

			if (returnPeriodInUtc.StartDateTime.Minute < 15)
			{
				returnPeriodInUtc =
					returnPeriodInUtc.ChangeStartTime(TimeSpan.FromMinutes(returnPeriodInUtc.StartDateTime.Minute));
			}

			returnPeriodInUtc = returnPeriodInUtc.ChangeStartTime(new TimeSpan(0, -15, 0));
			returnPeriodInUtc = returnPeriodInUtc.ChangeEndTime(new TimeSpan(0, 15, 0));
			return returnPeriodInUtc;
		}

		private DateTimePeriod? getScheduleMinMax(IEnumerable<AgentInTeamScheduleViewModel> agentSchedules)
		{
			var schedules = agentSchedules as IList<AgentInTeamScheduleViewModel> ?? agentSchedules.ToList();

			var schedulesWithoutEmptyLayerDays = schedules.Where(s => !s.ScheduleLayers.IsNullOrEmpty()).ToList();

			if (!schedulesWithoutEmptyLayerDays.Any())
				return null;

			var timeZone = _logonUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var startTime = schedulesWithoutEmptyLayerDays.Min(s => s.ScheduleLayers.First().Start);
			var endTime = schedulesWithoutEmptyLayerDays.Max(l => l.ScheduleLayers.Last().End);

			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startTime, endTime, timeZone);
		}
	}
}