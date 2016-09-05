using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
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

		private IRequestApprovalService _requestApprovalService;

		public ApproveRequestCommandHandler(IScheduleStorage scheduleStorage, IScheduleDifferenceSaver scheduleDictionarySaver, IPersonRequestCheckAuthorization personRequestCheckAuthorization, IDifferenceCollectionService<IPersistableScheduleData> differenceService, IPersonRequestRepository personRequestRepository, IRequestApprovalServiceFactory requestApprovalServiceFactory, ICurrentScenario currentScenario, IWriteProtectedScheduleCommandValidator writeProtectedScheduleCommandValidator)
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

			if (approveRequest(personRequest, command))
			{
				command.AffectedRequestId = command.PersonRequestId;
				command.IsReplySuccess = command.TryReplyMessage(personRequest);
			}
		}

		public IRequestApprovalService GetRequestApprovalService()
		{
			return _requestApprovalService;
		}

		private bool approveRequest(IPersonRequest personRequest, ApproveRequestCommand command)
		{
			if ((personRequest.IsDenied && !personRequest.IsWaitlisted) || personRequest.IsCancelled)
			{
				return invalidRequestState(personRequest, command);
			}

			var scheduleDictionary = getSchedules(personRequest);
			 _requestApprovalService = _requestApprovalServiceFactory.MakeRequestApprovalServiceScheduler(scheduleDictionary, _currentScenario.Current(), personRequest.Person);

			try
			{
				personRequest.Approve(_requestApprovalService, _personRequestCheckAuthorization);
			}
			catch (InvalidRequestStateTransitionException)
			{
				return invalidRequestState(personRequest, command);
			}

			foreach (var range in scheduleDictionary.Values)
			{

				var diff = range.DifferenceSinceSnapshot(_differenceService);
				_scheduleDictionarySaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate)range);
			}

			return true;
		}

		private static bool invalidRequestState(IPersonRequest personRequest, ApproveRequestCommand command)
		{
			command.ErrorMessages.Add(string.Format(UserTexts.Resources.RequestInvalidStateTransition, personRequest.StatusText,
				UserTexts.Resources.Approved));
			return false;
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
