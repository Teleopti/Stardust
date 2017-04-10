using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemoveSelectedPersonAbsenceCommandHandler :IHandleCommand<RemoveSelectedPersonAbsenceCommand>
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(RemoveSelectedPersonAbsenceCommandHandler));
		private readonly ICurrentScenario _currentScenario;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly IPersonAbsenceRemover _personAbsenceRemover;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public RemoveSelectedPersonAbsenceCommandHandler(ICurrentScenario currentScenario,
			IPersonAbsenceRepository personAbsenceRepository, IScheduleStorage scheduleStorage,
			IPersonRepository personRepository, IPersonAbsenceRemover personAbsenceRemover, ICurrentUnitOfWork currentUnitOfWork)
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
			var person = _personRepository.Get(command.PersonId);
			var personAbsences = _personAbsenceRepository.Get(command.PersonAbsenceId);
			if (personAbsences == null)
			{
				logger.Warn($"Could not find personAbsence with Id = {command.PersonAbsenceId}");
				appendErrorMessage(command.ErrorMessages, Resources.InvalidParameters);
				return;
			}

			var loadOptions = new ScheduleDictionaryLoadOptions(false, false);
			var period = command.Date.ToDateOnlyPeriod().Inflate(1);
			var currentScenario = _currentScenario.Current();
			var dictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, loadOptions, period, currentScenario);

			var scheduleRange = dictionary?[person];
			var scheduleDay = scheduleRange?.ScheduledDay(command.Date);
			if (scheduleDay == null)
			{
				logger.Warn($"No schedule found for {command.Date.Date:yyyy-MM-dd} and person \"{command.PersonId}\" "
							+ "in scenario \"{currentScenario.Id}\"");
				appendErrorMessage(command.ErrorMessages, Resources.InvalidParameters);
				return;
			}

			var personAssignment = scheduleDay.PersonAssignment();
			var periodToRemove = personAssignment != null && personAssignment.ShiftLayers.Any()
				? coverAllDay(personAssignment.Period, personAbsences.Period, person.PermissionInformation.DefaultTimeZone())
				: scheduleDay.Period;

			command.ErrorMessages = _personAbsenceRemover.RemovePartPersonAbsence(command.Date, person, personAbsences,
				periodToRemove, scheduleRange, command.TrackedCommandInfo).ToList();
			_currentUnitOfWork.Current().PersistAll();
		}

		private DateTimePeriod coverAllDay(DateTimePeriod period, DateTimePeriod absencePeriod, TimeZoneInfo timezone)
		{
			if (period.Contains(absencePeriod))
			{
				return period;
			}

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
			return period;
		}

		private void appendErrorMessage(IList<string> errorMessages, string newMessage)
		{
			if (errorMessages == null)
			{
				errorMessages = new List<string> ();
			}
			errorMessages.Add(newMessage);
		}
	}
}
