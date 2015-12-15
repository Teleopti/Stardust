using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ModifyPersonAbsenceCommandHandler : IHandleCommand<ModifyPersonAbsenceCommand>
	{
		private readonly IWriteSideRepositoryTypedId<IPersonAbsence, Guid> _personAbsenceRepository;
		private readonly IUserTimeZone _timeZone;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IBusinessRulesForPersonalAccountUpdate _businessRulesForPersonalAccountUpdate;
		private readonly ISaveSchedulePartService _saveSchedulePartService;

		public ModifyPersonAbsenceCommandHandler(IWriteSideRepositoryTypedId<IPersonAbsence, Guid> personAbsenceRepository,
			IUserTimeZone timeZone,
			IScheduleRepository scheduleRepository,
			IBusinessRulesForPersonalAccountUpdate businessRulesForPersonalAccountUpdate,
			ISaveSchedulePartService saveSchedulePartService)
		{
			_personAbsenceRepository = personAbsenceRepository;
			_timeZone = timeZone;
			_scheduleRepository = scheduleRepository;
			_businessRulesForPersonalAccountUpdate = businessRulesForPersonalAccountUpdate;
			_saveSchedulePartService = saveSchedulePartService;
		}

		public void Handle(ModifyPersonAbsenceCommand command)
		{
			
			var personAbsence = (PersonAbsence)_personAbsenceRepository.LoadAggregate(command.PersonAbsenceId);
			if (personAbsence == null)
				throw new InvalidOperationException(string.Format("Person Absence is not found. StartTime: {0} EndTime: {1} PersonId: {2} AbsenceId: {3}  ", command.StartTime, command.EndTime, command.PersonId, command.PersonAbsenceId));

			var person = personAbsence.Person;
			var absenceTimePeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(command.StartTime, _timeZone.TimeZone()),
													   TimeZoneHelper.ConvertToUtc(command.EndTime, _timeZone.TimeZone()));

			var startDate = new DateOnly(personAbsence.Period.StartDateTimeLocal(_timeZone.TimeZone()));
			var endDate = new DateOnly(personAbsence.Period.EndDateTimeLocal(_timeZone.TimeZone()));
			var existingPeriodOfAbsence = new DateOnlyPeriod (startDate, endDate);

			var scheduleDictionary =
					_scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(
						person, new ScheduleDictionaryLoadOptions(false, false),
						existingPeriodOfAbsence, personAbsence.Scenario);

			var scheduleRange = scheduleDictionary[person];
			var rules = _businessRulesForPersonalAccountUpdate.FromScheduleRange(scheduleRange);

			var scheduleDaysToUpdate =
				from day in existingPeriodOfAbsence.DayCollection()
				select scheduleRange.ScheduledDay (startDate) as ExtractedSchedule;

			personAbsence.ModifyPersonAbsencePeriod(absenceTimePeriod, command.TrackedCommandInfo);

			foreach (var scheduleDay in scheduleDaysToUpdate)
			{
				_saveSchedulePartService.Save(scheduleDay, rules, KeepOriginalScheduleTag.Instance);
			}

		}
	}
}
