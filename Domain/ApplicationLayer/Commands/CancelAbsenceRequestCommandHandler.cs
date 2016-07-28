using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class CancelAbsenceRequestCommandHandler : IHandleCommand<CancelAbsenceRequestCommand>
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IPersonAbsenceRepository _personAbsenceRepository;
		private readonly IPersonAbsenceRemover _personAbsenceRemover;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IWriteProtectedScheduleCommandValidator _writeProtectedScheduleCommandValidator;
		private readonly ICancelAbsenceRequestCommandValidator _cancelAbsenceRequestCommandValidator;

		public CancelAbsenceRequestCommandHandler(IPersonRequestRepository personRequestRepository,
			IPersonAbsenceRepository personAbsenceRepository, IPersonAbsenceRemover personAbsenceRemover,
			IScheduleStorage scheduleStorage, ICurrentScenario currentScenario,
			IWriteProtectedScheduleCommandValidator writeProtectedScheduleCommandValidator,
			ICancelAbsenceRequestCommandValidator cancelAbsenceRequestCommandValidator)
		{
			_personRequestRepository = personRequestRepository;
			_personAbsenceRepository = personAbsenceRepository;
			_personAbsenceRemover = personAbsenceRemover;
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_writeProtectedScheduleCommandValidator = writeProtectedScheduleCommandValidator;
			_cancelAbsenceRequestCommandValidator = cancelAbsenceRequestCommandValidator;
		}

		public void Handle(CancelAbsenceRequestCommand command)
		{
			command.ErrorMessages = new List<string>();

			var personRequest = _personRequestRepository.Get(command.PersonRequestId);
			if (personRequest == null)
			{
				return;
			}

			if (!_writeProtectedScheduleCommandValidator.ValidateCommand(personRequest.RequestedDate, personRequest.Person, command))
			{
				return;
			}

			if (!cancelRequest(personRequest, command)) return;

			command.AffectedRequestId = command.PersonRequestId;
			if (!command.ReplyMessage.IsNullOrEmpty())
			{
				if (tryReplyMessage(personRequest, command))
				{
					command.IsReplySuccess = true;
				}
			}
		}
		private bool tryReplyMessage(IPersonRequest personRequest, CancelAbsenceRequestCommand command)
		{
			try
			{
				if (!personRequest.CheckReplyTextLength(command.ReplyMessage))
				{
					command.ErrorMessages.Add(UserTexts.Resources.RequestInvalidMessageLength);
					return false;
				}
				personRequest.Reply(command.ReplyMessage);
			}
			catch (InvalidOperationException)
			{
				command.ErrorMessages.Add(string.Format(UserTexts.Resources.RequestInvalidMessageModification, personRequest.StatusText));
				return false;
			}
			return true;
		}

		private bool cancelRequest(IPersonRequest personRequest, CancelAbsenceRequestCommand command)
		{
			var absenceRequest = personRequest.Request as IAbsenceRequest;

			if (absenceRequest == null)
			{
				return false;
			}

			var personAbsences = _personAbsenceRepository.Find(personRequest, _currentScenario.Current());

			if (!_cancelAbsenceRequestCommandValidator.ValidateCommand(personRequest, command, absenceRequest, personAbsences))
			{
				return false;
			}

			try
			{
				var person = personRequest.Person;
				var startDate = personAbsences.Min(pa => pa.Period.StartDateTime);
				var endDate = personAbsences.Max(pa => pa.Period.EndDateTime);
				var scheduleRange = getScheduleRange(person, startDate, endDate);

				foreach (var personAbsence in personAbsences)
				{
					var startDateOnly = new DateOnly(personAbsence.Period.LocalStartDateTime);
					var errorMessages = _personAbsenceRemover.RemovePersonAbsence(startDateOnly,
						personRequest.Person, new[] { personAbsence }, scheduleRange).ToList();

					if (!errorMessages.Any()) continue;

					command.ErrorMessages = command.ErrorMessages.Concat(errorMessages).ToList();
					return false;
				}

				return true;
			}
			catch (InvalidRequestStateTransitionException)
			{
				command.ErrorMessages.Add(string.Format(UserTexts.Resources.RequestInvalidStateTransition, personRequest.StatusText, UserTexts.Resources.Cancelled));
			}

			return false;
		}

		private IScheduleRange getScheduleRange(IPerson person, DateTime startDate, DateTime endDate)
		{
			var scheduleDictionary =
				_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(false, false),
					new DateTimePeriod(startDate, endDate),
					_currentScenario.Current());

			return scheduleDictionary[person];
		}
	}
}
