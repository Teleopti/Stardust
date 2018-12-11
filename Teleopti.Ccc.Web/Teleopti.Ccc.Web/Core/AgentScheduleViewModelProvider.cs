using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;

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
		private readonly ITeamScheduleShiftViewModelProvider _shiftViewModelProvider;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly TeamScheduleAgentScheduleViewModelMapper _layerMapper;

		public AgentScheduleViewModelProvider(IScheduleStorage scheduleStorage, IPersonRepository personRepository, ICurrentScenario currentScenario, 
			ITeamScheduleShiftViewModelProvider shiftViewModelProvider, ILoggedOnUser loggedOnUser, TeamScheduleAgentScheduleViewModelMapper layerMapper)
		{
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_currentScenario = currentScenario;
			_shiftViewModelProvider = shiftViewModelProvider;
			_loggedOnUser = loggedOnUser;
			_layerMapper = layerMapper;
		}

		public ShiftTradeSchedulesViewModel GetShiftTradeSchedulesViewModel(ShiftTradeScheduleForm input)
		{
			var personFrom = _personRepository.Get(input.PersonFromId);
			var personTo = _personRepository.Get(input.PersonToId);
			var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new List<IPerson> {personFrom, personTo},
				new ScheduleDictionaryLoadOptions(false, false), input.RequestDate.ToDateOnly().ToDateOnlyPeriod(), 
				_currentScenario.Current());

			var fromScheduleDay = schedules[personFrom].ScheduledDay(input.RequestDate.ToDateOnly());
			var toScheduleDay = schedules[personTo].ScheduledDay(input.RequestDate.ToDateOnly());
			return new ShiftTradeSchedulesViewModel
			{
				PersonFromSchedule = GetAgentScheduleViewModel(personFrom, fromScheduleDay),
				PersonToSchedule = GetAgentScheduleViewModel(personTo, toScheduleDay)
			};
		}

		public TeamScheduleAgentScheduleViewModel GetAgentScheduleViewModel(IPerson person, IScheduleDay scheduleDay)
		{
			var scheduleViewModel = _shiftViewModelProvider.MakeScheduleReadModel(person, scheduleDay, true);
			var schedulePeriod = getScheduleMinMax(scheduleViewModel, person, scheduleDay.DateOnlyAsPeriod.DateOnly);
			var isMySchedule = _loggedOnUser.CurrentUser().Equals(person);

			return _layerMapper.Map(scheduleViewModel, schedulePeriod, isMySchedule);
		}

		private DateTimePeriod getScheduleMinMax(AgentInTeamScheduleViewModel schedule, IPerson person, DateOnly date)
		{
			var timeZone = person.PermissionInformation.DefaultTimeZone();

			if (schedule.ScheduleLayers.IsNullOrEmpty())
			{
				return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date.Date.AddHours(DefaultSchedulePeriodProvider.DefaultStartHour), date.Date.AddHours(DefaultSchedulePeriodProvider.DefaultEndHour), timeZone);
			}

			var startTime = schedule.ScheduleLayers.First().Start;
			var endTime = schedule.ScheduleLayers.Last().End;

			return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(startTime, endTime, _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());
		}
	}
}