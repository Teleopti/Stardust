using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddFullDayAbsenceCommandHandler : IHandleCommand<AddFullDayAbsenceCommand>
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentScenario _scenario;
		private readonly IWriteSideRepository<IPerson> _personRepository;
		private readonly IWriteSideRepository<IAbsence> _absenceRepository;
		private readonly IWriteSideRepository<IPersonAbsence> _personAbsenceRepository;
		private readonly IScheduleRepository _scheduleRepository;

		public AddFullDayAbsenceCommandHandler(ICurrentDataSource dataSource, ICurrentScenario scenario, IWriteSideRepository<IPerson> personRepository, IWriteSideRepository<IAbsence> absenceRepository, IWriteSideRepository<IPersonAbsence> personAbsenceRepository, IScheduleRepository scheduleRepository)
		{
			_dataSource = dataSource;
			_scenario = scenario;
			_personRepository = personRepository;
			_absenceRepository = absenceRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_scheduleRepository = scheduleRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(AddFullDayAbsenceCommand command)
		{
			var person = _personRepository.Load(command.PersonId);
			var absence = _absenceRepository.Load(command.AbsenceId);
			var period = new DateOnlyPeriod(new DateOnly(command.StartDate.AddDays(-1)), new DateOnly(command.EndDate));
			var scheduleDays = getScheduleDaysForPeriod(period, person);

			var previousDay = scheduleDays.First();
			var absenceTimePeriod = getDateTimePeriodForAbsence(scheduleDays.Except(new[] {previousDay}), previousDay);

			var personAbsence = new PersonAbsence(_scenario.Current());
			personAbsence.FullDayAbsence(_dataSource.CurrentName(), person, absence, absenceTimePeriod.StartDateTime, absenceTimePeriod.EndDateTime);

			_personAbsenceRepository.Add(personAbsence);
		}

		private IEnumerable<IScheduleDay> getScheduleDaysForPeriod(DateOnlyPeriod period, IPerson person)
		{
			var timeZone = person.PermissionInformation.DefaultTimeZone();
			var dateTimePeriod = period.ToDateTimePeriod(timeZone);

			var dictionary = _scheduleRepository.FindSchedulesOnlyInGivenPeriod(
				new PersonProvider(new[] {person}),
				new ScheduleDictionaryLoadOptions(false, false),
				dateTimePeriod,
				_scenario.Current());

			var scheduleDays = dictionary[person].ScheduledDayCollection(period);
			return scheduleDays;
		}

		private DateTimePeriod getDateTimePeriodForAbsence(IEnumerable<IScheduleDay> scheduleDaysInPeriod, IScheduleDay previousDay)
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