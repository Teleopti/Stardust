using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemoveSelectedPersonAbsenceCommandHandler :IHandleCommand<RemoveSelectedPersonAbsenceCommand>
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(RemoveSelectedPersonAbsenceCommandHandler));
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IPersonAbsenceRemover _personAbsenceRemover;

		public RemoveSelectedPersonAbsenceCommandHandler(IPersonAbsenceRepository personAbsenceRepository, IPersonAbsenceRemover personAbsenceRemover)
		{
			_personAbsenceRepository = personAbsenceRepository;
			_personAbsenceRemover = personAbsenceRemover;
		}

		public void Handle(RemoveSelectedPersonAbsenceCommand command)
		{
			var person = command.Person;
			var personAbsences = _personAbsenceRepository.Get(command.PersonAbsenceId);
			if (personAbsences == null)
			{
				logger.Warn($"Could not find personAbsence with Id = {command.PersonAbsenceId}");
				appendErrorMessage(command.ErrorMessages, Resources.InvalidParameters);
				return;
			}

			var scheduleRange = command.ScheduleRange;
			var scheduleDay = scheduleRange?.ScheduledDay(command.Date);
			if (scheduleDay == null)
			{
				logger.Warn($"No schedule found for {command.Date.Date:yyyy-MM-dd} and person \"{command.Person.Id.GetValueOrDefault()}\" "
							+ "in scenario \"{currentScenario.Id}\"");
				appendErrorMessage(command.ErrorMessages, Resources.InvalidParameters);
				return;
			}

			var personAssignment = scheduleDay.PersonAssignment();
			var previousPersonAssignmentPeriod = scheduleRange?.ScheduledDay(command.Date.AddDays(-1))?.PersonAssignment()?.Period;

			var period = personAssignment == null || !personAssignment.ShiftLayers.Any() ? scheduleDay.Period : personAssignment.Period;
			var periodToRemove = excludeOvernightFromPreviousDay(previousPersonAssignmentPeriod, period.Contains(personAbsences.Period)
					? period : coverWholeDayAndOvernight(period, person.PermissionInformation.DefaultTimeZone()));

			command.ErrorMessages = _personAbsenceRemover.RemovePartPersonAbsence(command.Date, person, personAbsences,
				periodToRemove, scheduleRange, command.TrackedCommandInfo).ToList();
		}

		private DateTimePeriod excludeOvernightFromPreviousDay(DateTimePeriod? previousPeriod, DateTimePeriod period)
		{
			if (previousPeriod == null)
				return period;

			var prePeriod = previousPeriod.GetValueOrDefault();

			var previousDayIsOvernight = period.ContainsPart(prePeriod);

			if (previousDayIsOvernight)
			{
				return new DateTimePeriod(prePeriod.EndDateTime, period.EndDateTime);
			}

			return period;
		}

		private DateTimePeriod coverWholeDayAndOvernight(DateTimePeriod period, TimeZoneInfo timezone)
		{
			var startTimeLocal = period.StartDateTimeLocal(timezone);
			var endTimeLocal = period.EndDateTimeLocal(timezone);
			var startOfDay = startTimeLocal.Date;
			var endOfDay = startOfDay.AddDays(1);

			var overnight = endTimeLocal.CompareTo(endOfDay) > 0;

			var startAdjusted = TimeZoneHelper.ConvertToUtc(startOfDay, timezone);
			var endAdjusted = TimeZoneHelper.ConvertToUtc(overnight ? endTimeLocal : endOfDay, timezone);

			return new DateTimePeriod(startAdjusted, endAdjusted);
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
