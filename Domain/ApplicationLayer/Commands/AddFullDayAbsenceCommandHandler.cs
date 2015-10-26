using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddFullDayAbsenceCommandHandler : IHandleCommand<AddFullDayAbsenceCommand>
	{
		private readonly ICurrentScenario _scenario;
		private readonly IProxyForId<IPerson> _personRepository;
		private readonly IProxyForId<IAbsence> _absenceRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IPersonAbsenceCreator _personAbsenceCreator;

		public AddFullDayAbsenceCommandHandler(IScheduleRepository scheduleRepository, 
			IProxyForId<IPerson> personRepository, 
			IProxyForId<IAbsence> absenceRepository, 
			ICurrentScenario scenario, 
			IPersonAbsenceCreator personAbsenceCreator)
		{
			_scenario = scenario;
			_personAbsenceCreator = personAbsenceCreator;
			_personRepository = personRepository;
			_absenceRepository = absenceRepository;
			_scheduleRepository = scheduleRepository;
		}

		public void Handle(AddFullDayAbsenceCommand command)
		{
			var person = _personRepository.Load(command.PersonId);
			var absence = _absenceRepository.Load(command.AbsenceId);
			var period = new DateOnlyPeriod(new DateOnly(command.StartDate.AddDays(-1)), new DateOnly(command.EndDate));
			
			var scheduleRange = getScheduleRangeForPeriod(period, person);
			var scheduleDays = scheduleRange.ScheduledDayCollection(period).ToList();
			var scheduleDay = scheduleRange.ScheduledDay(new DateOnly(command.StartDate));

			var previousDay = scheduleDays.First();
			var absenceTimePeriod = getDateTimePeriodForAbsence(scheduleDays.Except(new[] {previousDay}).ToList(), previousDay);

			_personAbsenceCreator.Create(absence, scheduleRange, scheduleDay, absenceTimePeriod, person);
			
		}

		private IScheduleRange getScheduleRangeForPeriod(DateOnlyPeriod period, IPerson person)
		{
			var dictionary = _scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(
					person,
					new ScheduleDictionaryLoadOptions(false, false),
					period,
					_scenario.Current());

			return dictionary[person];
		}

		private DateTimePeriod getDateTimePeriodForAbsence(IList<IScheduleDay> scheduleDaysInPeriod, IScheduleDay previousDay)
		{
			var firstDayDateTime = getDateTimePeriodForDay(scheduleDaysInPeriod.First());
			var lastDayDateTime = getDateTimePeriodForDay(scheduleDaysInPeriod.Last());
			var previousday = getDateTimePeriodForDay(previousDay);
			var startTime = previousday.EndDateTime > firstDayDateTime.StartDateTime ? previousday.EndDateTime : firstDayDateTime.StartDateTime;
			var endTime = lastDayDateTime.EndDateTime;
			return new DateTimePeriod(startTime, endTime);
		}

		private DateTimePeriod getDateTimePeriodForDay(IScheduleDay scheduleDay)
		{
			var startTime = DateTime.MaxValue;
			var endTime = DateTime.MinValue;
			var personAssignment = scheduleDay.PersonAssignment();

			if (personAssignment != null)
			{
				var personAssignmentStartDateTime = personAssignment.Period.StartDateTime;
				var personAssignmentEndDateTime = personAssignment.Period.EndDateTime;

				if (personAssignmentStartDateTime <= startTime)
				{
					startTime = personAssignmentStartDateTime;
				}

				if (personAssignmentEndDateTime >= endTime)
				{
					endTime = personAssignmentEndDateTime;
				}
			}
			else
			{
				startTime = scheduleDay.Period.StartDateTime;
				endTime = scheduleDay.Period.EndDateTime.AddMinutes(-1);
			}

			return new DateTimePeriod(startTime, endTime);
		}
	}
}