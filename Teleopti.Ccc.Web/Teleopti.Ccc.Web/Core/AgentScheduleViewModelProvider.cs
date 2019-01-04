using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Core.Extensions;

namespace Teleopti.Ccc.Web.Core
{
	public interface IAgentScheduleViewModelProvider
	{
		ShiftTradeSchedulesViewModel GetShiftTradeSchedulesViewModel(ShiftTradeScheduleForm input);
		TeamScheduleAgentScheduleViewModel GetAgentScheduleViewModel(IPerson person, IScheduleDay scheduleDay);
	}

	public class AgentScheduleViewModelProvider : IAgentScheduleViewModelProvider
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly TeamScheduleShiftViewModelProvider _shiftViewModelProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly TeamScheduleAgentScheduleViewModelMapper _layerMapper;
		private readonly ITimeLineViewModelFactory _timeLineViewModelFactory;

		public AgentScheduleViewModelProvider(IScheduleStorage scheduleStorage, IPersonRepository personRepository, ICurrentScenario currentScenario,
			TeamScheduleShiftViewModelProvider shiftViewModelProvider, ILoggedOnUser loggedOnUser, TeamScheduleAgentScheduleViewModelMapper layerMapper, ITimeLineViewModelFactory timeLineViewModelFactory)
		{
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_currentScenario = currentScenario;
			_shiftViewModelProvider = shiftViewModelProvider;
			_loggedOnUser = loggedOnUser;
			_layerMapper = layerMapper;
			_timeLineViewModelFactory = timeLineViewModelFactory;
		}

		private DateTimePeriod? getScheduleMinMax(IEnumerable<AgentInTeamScheduleViewModel> agentSchedules)
		{
			var schedules = agentSchedules as IList<AgentInTeamScheduleViewModel> ?? agentSchedules.ToList();

			var schedulesWithoutEmptyLayerDays = schedules.Where(s => !s.ScheduleLayers.IsNullOrEmpty()).ToList();

			if (!schedulesWithoutEmptyLayerDays.Any())
				return null;

			var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();

			var startTime = schedulesWithoutEmptyLayerDays.Min(s => s.ScheduleLayers.First().Start);
			var endTime = schedulesWithoutEmptyLayerDays.Max(l => l.ScheduleLayers.Last().End);

			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startTime, endTime, timeZone);
		}

		private DateTimePeriod getSchedulePeriod(IPerson currentUser, IEnumerable<AgentInTeamScheduleViewModel> agentSchedules, DateOnly date)
		{
			var scheduleMinMaxPeriod = getScheduleMinMax(agentSchedules);

			var timeZone = currentUser.PermissionInformation.DefaultTimeZone();

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
			return returnPeriodInUtc;
		}

		public ShiftTradeSchedulesViewModel GetShiftTradeSchedulesViewModel(ShiftTradeScheduleForm input)
		{
			var personFrom = _personRepository.Get(input.PersonFromId);
			var personTo = _personRepository.Get(input.PersonToId);
			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new List<IPerson> { personFrom, personTo },
				new ScheduleDictionaryLoadOptions(false, false), input.RequestDate.ToDateOnly().ToDateOnlyPeriod(),
				_currentScenario.Current());

			var fromScheduleDay = schedules[personFrom].ScheduledDay(input.RequestDate.ToDateOnly());
			var toScheduleDay = schedules[personTo].ScheduledDay(input.RequestDate.ToDateOnly());

			var personFromSchedule = GetAgentInTeamScheduleViewModel(personFrom, fromScheduleDay);
			var personToSchedule = GetAgentInTeamScheduleViewModel(personTo, toScheduleDay);

			var agentSchedules = new List<AgentInTeamScheduleViewModel> { personFromSchedule, personToSchedule };

			var currentUser = _loggedOnUser.CurrentUser();
			var schedulePeriodInUtc = getSchedulePeriod(currentUser, agentSchedules, input.RequestDate.ToDateOnly());

			var timeLine = _timeLineViewModelFactory.CreateTimeLineHours(schedulePeriodInUtc, input.RequestDate.ToDateOnly())
								.Select(t => new TeamScheduleTimeLineViewModel
								{
									TimeLineDisplay = t.Time.ToLocalizedTimeFormat(),
									Time = t.Time,
									PositionPercentage = t.PositionPercentage,
									TimeFixedFormat = t.TimeFixedFormat
								}).ToArray();

			return new ShiftTradeSchedulesViewModel
			{
				TimeLine = timeLine,
				PersonFromSchedule = GetAgentScheduleViewModel(currentUser, personFromSchedule, personFrom, schedulePeriodInUtc),
				PersonToSchedule = GetAgentScheduleViewModel(currentUser, personToSchedule,personTo, schedulePeriodInUtc)
			};
		}

		private TeamScheduleAgentScheduleViewModel GetAgentScheduleViewModel(IPerson currentUser, AgentInTeamScheduleViewModel scheduleViewModel, IPerson person, DateTimePeriod schedulePeriod)
		{
			var isMySchedule = currentUser.Equals(person);
			return _layerMapper.Map(scheduleViewModel, schedulePeriod, isMySchedule);
		}

		private AgentInTeamScheduleViewModel GetAgentInTeamScheduleViewModel(IPerson person, IScheduleDay scheduleDay)
		{
			var scheduleViewModel = _shiftViewModelProvider.MakeScheduleReadModel(_loggedOnUser.CurrentUser(), person, scheduleDay, true);
			return scheduleViewModel;
		}

		public TeamScheduleAgentScheduleViewModel GetAgentScheduleViewModel(IPerson person, IScheduleDay scheduleDay)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var scheduleViewModel = _shiftViewModelProvider.MakeScheduleReadModel(currentUser, person, scheduleDay, true);
			var schedulePeriod = getScheduleMinMax(currentUser, scheduleViewModel, person, scheduleDay.DateOnlyAsPeriod.DateOnly);
			var isMySchedule = currentUser.Equals(person);

			return _layerMapper.Map(scheduleViewModel, schedulePeriod, isMySchedule);
		}

		private DateTimePeriod getScheduleMinMax(IPerson currentUser, AgentInTeamScheduleViewModel schedule, IPerson person, DateOnly date)
		{
			var timeZone = person.PermissionInformation.DefaultTimeZone();

			if (schedule.ScheduleLayers.IsNullOrEmpty())
			{
				return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date.Date.AddHours(DefaultSchedulePeriodProvider.DefaultStartHour), date.Date.AddHours(DefaultSchedulePeriodProvider.DefaultEndHour), timeZone);
			}

			var startTime = schedule.ScheduleLayers.First().Start;
			var endTime = schedule.ScheduleLayers.Last().End;

			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startTime, endTime, currentUser.PermissionInformation.DefaultTimeZone());
		}
	}
}