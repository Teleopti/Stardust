using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemoveSelectedPersonAbsenceCommandHandler :IHandleCommand<RemoveSelectedPersonAbsenceCommand>
	{
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly IPersonAbsenceRemover _personAbsenceRemover;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public RemoveSelectedPersonAbsenceCommandHandler(ICurrentScenario currentScenario, IPersonAbsenceRepository personAbsenceRepository, IScheduleStorage scheduleStorage, IPersonRepository personRepository, IPersonAbsenceRemover personAbsenceRemover, ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentScenario = currentScenario;
			_personAbsenceRepository = personAbsenceRepository;
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_personAbsenceRemover = personAbsenceRemover;
			_currentUnitOfWork = currentUnitOfWork;
		}
		
		public void Handle(RemoveSelectedPersonAbsenceCommand command)
		{
			var scenario = _currentScenario.Current();
			var person = _personRepository.Get(command.PersonId);
			var personAbsences = _personAbsenceRepository.Get(command.PersonAbsenceId);
			
			var period = command.Date.ToDateOnlyPeriod().Inflate(1);

			var dictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(
					person,
					new ScheduleDictionaryLoadOptions(false,false),
					period,
					scenario);

			var scheduleRange = dictionary[person];
			var scheduleDay = scheduleRange.ScheduledDay(command.Date);			
			var personAssignment = scheduleDay.PersonAssignment();

			var periodToRemove = personAssignment != null && personAssignment.ShiftLayers.Any()
				? coverAllDay(personAssignment.Period, personAbsences.Period, person.PermissionInformation.DefaultTimeZone())
				: scheduleDay.Period;

			command.ErrorMessages =
				_personAbsenceRemover.RemovePartPersonAbsence(command.Date, person, personAbsences, periodToRemove, scheduleRange,
					command.TrackedCommandInfo).ToList();
			_currentUnitOfWork.Current().PersistAll();
		}

		private DateTimePeriod coverAllDay(DateTimePeriod period, DateTimePeriod absencePeriod, TimeZoneInfo timezone)
		{
			var contains = period.Contains(absencePeriod);
			if (!contains)
			{
				var startTimeLocal = period.StartDateTimeLocal(timezone);
				var endTimeLocal = period.EndDateTimeLocal(timezone);

				var laterThanZero =
					endTimeLocal.CompareTo(new DateTime(endTimeLocal.Year, endTimeLocal.Month, endTimeLocal.Day, 0, 0, 0)) > 0;
				if (laterThanZero)
				{
					endTimeLocal = endTimeLocal.AddDays(1);
				}

				var startAdjusted =
					TimeZoneHelper.ConvertToUtc(new DateTime(startTimeLocal.Year, startTimeLocal.Month, startTimeLocal.Day, 0, 0, 0),
						timezone);
				var endAdjusted =
					TimeZoneHelper.ConvertToUtc(new DateTime(endTimeLocal.Year, endTimeLocal.Month, endTimeLocal.Day, 0, 0, 0),
						timezone);

				period = new DateTimePeriod(startAdjusted, endAdjusted);
			}
			return period;
		}
	}
}
