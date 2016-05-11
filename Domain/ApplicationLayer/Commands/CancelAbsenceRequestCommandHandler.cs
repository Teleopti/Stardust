using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
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

		public CancelAbsenceRequestCommandHandler(IPersonRequestRepository personRequestRepository, IPersonAbsenceRepository personAbsenceRepository, IPersonAbsenceRemover personAbsenceRemover, ILoggedOnUser loggedOnUser, IUserCulture userCulture, IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IWriteProtectedScheduleCommandValidator writeProtectedScheduleCommandValidator, ICancelAbsenceRequestCommandValidator cancelAbsenceRequestCommandValidator )
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
			
			if (!_writeProtectedScheduleCommandValidator.ValidateCommand (personRequest.RequestedDate, personRequest.Person, command))
			{
				return;
			}

			if (cancelRequest(personRequest, command))
			{
				command.AffectedRequestId = command.PersonRequestId;
			}
		}
		
		private bool cancelRequest(IPersonRequest personRequest, CancelAbsenceRequestCommand command)
		{
			var absenceRequest = personRequest.Request as IAbsenceRequest;

			if (absenceRequest == null)
			{
				return false;
			}

			var personAbsences = _personAbsenceRepository.Find(absenceRequest);

			if (!_cancelAbsenceRequestCommandValidator.ValidateCommand(personRequest, command, absenceRequest, personAbsences))
			{
				return false;
			}

			try
			{
				var person = personRequest.Person;
				var startDate = personAbsences.Min(pa => pa.Period.StartDateTime);
				var endDate = personAbsences.Max (pa => pa.Period.EndDateTime);
				var scheduleRange = getScheduleRange(person, startDate, endDate);

				IList<string> errorMessages = new List<string>();

				foreach (var personAbsence in personAbsences)
				{
					errorMessages = _personAbsenceRemover.RemovePersonAbsence (new DateOnly (personAbsence.Period.LocalStartDateTime),
						personRequest.Person,new[] {personAbsence},scheduleRange).ToList();

					if (errorMessages.Any())
					{
						command.ErrorMessages = command.ErrorMessages.Concat (errorMessages).ToList() ;
						return false;
					}
				}

				return true;
			}

			catch (InvalidRequestStateTransitionException)
			{
				command.ErrorMessages.Add(string.Format(UserTexts.Resources.RequestInvalidStateTransititon, personRequest.StatusText, UserTexts.Resources.Cancelled));
			}

			return false;
		}

		private IScheduleRange getScheduleRange (IPerson person, DateTime startDate, DateTime endDate)
		{
			var scheduleDictionary =
				_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod (person,
					new ScheduleDictionaryLoadOptions (false, false),
					new DateTimePeriod (startDate, endDate),
					_currentScenario.Current());

			return scheduleDictionary[person];
		}
	}
	
}
