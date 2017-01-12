using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class AddOverTimeHandler : IHandleEvent<AddOverTimeEvent>, IRunOnStardust
	{
		private readonly ScheduleOvertime _scheduleOvertime;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly ILoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly INow _now;

		public AddOverTimeHandler(ScheduleOvertime scheduleOvertime, 
			IScheduleStorage scheduleStorage, IPersonRepository personRepository, 
			INow now , ICurrentScenario currentScenario, 
			ILoadSchedulingStateHolderForResourceCalculation loadSchedulingStateHolderForResourceCalculation, ISchedulerStateHolder schedulerStateHolder)
		{
			_scheduleOvertime = scheduleOvertime;
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_now = now;
			_currentScenario = currentScenario;
			_loadSchedulingStateHolderForResourceCalculation = loadSchedulingStateHolderForResourceCalculation;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void Handle(AddOverTimeEvent @event)
		{
			// DUMMY CODE TO START WITH SOMETHING - SHOULD NOT BE CONSIDERED AS CODE TO USE
			//var overTimePreferences = new OvertimePreferences {SelectedTimePeriod = @event.Period.TimePeriod(TimeZoneInfo.Utc), ScheduleTag = new ScheduleTag()};
			//var persons = _personRepository.FindPeopleInOrganization(@event.Period.ToDateOnlyPeriod(TimeZoneInfo.Utc),false);

			//var scheduleRange = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(persons, new ScheduleDictionaryLoadOptions(false, false), @event.Period.ToDateOnlyPeriod(TimeZoneInfo.Utc), _currentScenario.Current());
			//IList<IScheduleDay> scheduleDays = new List<IScheduleDay>();

			//foreach (var person in persons)
			//{
			//	var sDays = scheduleRange[person].ScheduledDayCollection(@event.Period.ToDateOnlyPeriod(TimeZoneInfo.Utc));
			//	foreach (var day in sDays)
			//	{
			//		scheduleDays.Add(day);
			//	}
			//}

			//_schedulerStateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(@event.Period.ToDateOnlyPeriod(TimeZoneInfo.Utc), TimeZoneInfo.Utc);

			//_loadSchedulingStateHolderForResourceCalculation.Execute(_currentScenario.Current(),
			//											 @event.Period,
			//											 persons, _schedulerStateHolder.SchedulingResultState);

			//_scheduleOvertime.Execute(overTimePreferences, new NoSchedulingProgress(), scheduleDays);
		}
	}
}
