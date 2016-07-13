using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ApproveRequestCommandHandler : IHandleCommand<ApproveRequestCommand>
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IRequestApprovalServiceFactory _requestApprovalServiceFactory;
		private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceService;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IWriteProtectedScheduleCommandValidator _writeProtectedScheduleCommandValidator;
		
		public ApproveRequestCommandHandler(IScheduleStorage scheduleStorage,
			IScheduleDifferenceSaver scheduleDictionarySaver,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization,
			IDifferenceCollectionService<IPersistableScheduleData> differenceService,
			IPersonRequestRepository personRequestRepository,
			IRequestApprovalServiceFactory requestApprovalServiceFactory,
			ICurrentScenario currentScenario,
			IWriteProtectedScheduleCommandValidator writeProtectedScheduleCommandValidator)
		{
			_scheduleStorage = scheduleStorage;
			_scheduleDictionarySaver = scheduleDictionarySaver;
			_personRequestCheckAuthorization = personRequestCheckAuthorization;
			_differenceService = differenceService;

			_personRequestRepository = personRequestRepository;
			_requestApprovalServiceFactory = requestApprovalServiceFactory;
			_currentScenario = currentScenario;
			_writeProtectedScheduleCommandValidator = writeProtectedScheduleCommandValidator;
		}

		public void Handle(ApproveRequestCommand command)
		{
			var affectedRequestIds = new List<Guid>();
			var errorMessages = new List<string>();

			foreach (var personRequestId in command.PersonRequestIds)
			{
				var personRequest = _personRequestRepository.Get(personRequestId);

				if (personRequest == null)
				{
					continue;
				}

				if (!_writeProtectedScheduleCommandValidator.ValidateCommand(personRequest.RequestedDate,
					personRequest.Person, command))
				{
					continue;
				}

				string errorMessage;
				if (!approveRequest(personRequest, out errorMessage))
				{
					errorMessages.Add(errorMessage);
					continue;
				}

				affectedRequestIds.Add(personRequestId);
			}

			command.AffectedRequestIds = affectedRequestIds;
			command.ErrorMessages = errorMessages;
		}

		private bool approveRequest(IPersonRequest personRequest, out string errorMessage)
		{
			var scheduleDictionary = getSchedules(personRequest);
			var approvalService = _requestApprovalServiceFactory.MakeRequestApprovalServiceScheduler(scheduleDictionary,
				_currentScenario.Current(), personRequest.Person);

			try
			{
				personRequest.Approve(approvalService, _personRequestCheckAuthorization);
			}
			catch (InvalidRequestStateTransitionException)
			{
				errorMessage = string.Format(Resources.RequestInvalidStateTransition, personRequest.StatusText,
					Resources.Approved);
				return false;
			}

			foreach (var range in scheduleDictionary.Values)
			{
				var diff = range.DifferenceSinceSnapshot(_differenceService);
				_scheduleDictionarySaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate) range);
			}

			errorMessage = null;
			return true;
		}

		private IScheduleDictionary getSchedules(IPersonRequest personRequest)
		{
			var personList = new List<IPerson>();

			var absenceRequest = personRequest.Request as IAbsenceRequest;
			if (absenceRequest != null)
			{
				personList.Add(absenceRequest.Person);

			}
			var shiftTradeRequest = personRequest.Request as IShiftTradeRequest;
			if (shiftTradeRequest != null)
			{
				personList.AddRange(shiftTradeRequest.InvolvedPeople());
			}
			var scheduleDictionary = getScheduleDictionary(personRequest, personList);
			return scheduleDictionary;
		}

		private IScheduleDictionary getScheduleDictionary(IPersonRequest personRequest, IEnumerable<IPerson> personList)
		{
			var timePeriod = personRequest.Request.Period;
			var dateonlyPeriod = new DateOnlyPeriod(new DateOnly(timePeriod.StartDateTime.AddDays(-1)),
													new DateOnly(timePeriod.EndDateTime.AddDays(1)));
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				personList,
				new ScheduleDictionaryLoadOptions(true, false),
				dateonlyPeriod,
				_currentScenario.Current());
			((IReadOnlyScheduleDictionary)scheduleDictionary).MakeEditable();
			return scheduleDictionary;
		}
	}
}
