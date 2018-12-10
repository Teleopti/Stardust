using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ApproveRequestCommandHandler : IHandleCommand<ApproveRequestCommand>
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ApproveRequestCommandHandler));
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IRequestApprovalServiceFactory _requestApprovalServiceFactory;
		private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;
		private readonly IPersonRequestCheckAuthorization _personRequestCheckAuthorization;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceService;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IWriteProtectedScheduleCommandValidator _writeProtectedScheduleCommandValidator;

		private IRequestApprovalService _requestApprovalService;

		public ApproveRequestCommandHandler(IScheduleStorage scheduleStorage, IScheduleDifferenceSaver scheduleDictionarySaver,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization,
			IDifferenceCollectionService<IPersistableScheduleData> differenceService,
			IPersonRequestRepository personRequestRepository, IRequestApprovalServiceFactory requestApprovalServiceFactory,
			ICurrentScenario currentScenario, IWriteProtectedScheduleCommandValidator writeProtectedScheduleCommandValidator)
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

			if (personRequest == null || (personRequest.IsApproved && command.IgnoreErrorMessageForApprovedRequest))
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
			if (personRequest.IsApproved || personRequest.IsDeleted || (personRequest.IsDenied && !personRequest.IsWaitlisted) || personRequest.IsCancelled)
			{
				return invalidRequestState(personRequest, command);
			}

			var scheduleDictionary = getSchedules(personRequest);
			_requestApprovalService = getRequestApprovalService(personRequest, scheduleDictionary, command);

			IList<IBusinessRuleResponse> brokenRuleResponses;
			try
			{
				brokenRuleResponses = personRequest.Approve(_requestApprovalService, _personRequestCheckAuthorization,
					command.IsAutoGrant);
			}
			catch (InvalidRequestStateTransitionException)
			{
				return invalidRequestState(personRequest, command);
			}

			var anyRuleBroken = brokenRuleResponses != null && brokenRuleResponses.Any();

			if (anyRuleBroken)
			{
				command.ErrorMessages.Clear();
				brokenRuleResponses.ForEach(b => command.ErrorMessages.Add(b.Message));
			}

			var scheduleChangedWithBrokenRules = false;
			foreach (var range in scheduleDictionary.Values)
			{
				var diff = range.DifferenceSinceSnapshot(_differenceService);

				if (diff.Any())
				{
					if (!personRequest.IsApproved)
					{
						logger.Warn($"Schedule from {range.Period.StartDateTime:yyyy-mm-dd} to {range.Period.EndDateTime:yyyy-mm-dd} "
									+ "was changed on approving the request with Id=\"{personRequest.Id}\", "
									+ "but request status (\"{personRequest.StatusText}\") is not approved.");
					}

					if (anyRuleBroken)
					{
						scheduleChangedWithBrokenRules = true;
						logger.Warn($"Total {brokenRuleResponses.Count} business rules broken on approving "
									+ "the request with Id=\"{personRequest.Id}\", "
									+ $"but the schedule from {range.Period.StartDateTime:yyyy-mm-dd} "
									+ $"to {range.Period.EndDateTime:yyyy-mm-dd} was changed.");
					}
				}

				_scheduleDictionarySaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate)range);
			}

			if (scheduleChangedWithBrokenRules)
			{
				var messageBuilder = new StringBuilder();
				messageBuilder.AppendLine($"Total {brokenRuleResponses.Count} business rules broken:");

				var index = 1;
				foreach (var ruleResponse in brokenRuleResponses)
				{
					messageBuilder.AppendLine($"  {index}. [{ruleResponse.FriendlyName}] {ruleResponse.Message}");
					index++;
				}
				logger.Warn(messageBuilder.ToString());
			}

			return !anyRuleBroken;
		}

		private static bool invalidRequestState(IPersonRequest personRequest, ApproveRequestCommand command)
		{
			if (personRequest.IsDeleted)
			{
				command.ErrorMessages.Add(Resources.RequestHasBeenDeleted);
			}
			else
			{
				command.ErrorMessages.Add(string.Format(Resources.RequestInvalidStateTransition, personRequest.StatusText,
					Resources.Approved));
			}

			return false;
		}

		private IScheduleDictionary getSchedules(IPersonRequest personRequest)
		{
			var personList = new List<IPerson>();
			if (personRequest.Request.RequestType == RequestType.AbsenceRequest ||
			personRequest.Request.RequestType == RequestType.OvertimeRequest)
			{
				personList.Add(personRequest.Request.Person);
			}
			if (personRequest.Request.RequestType == RequestType.ShiftTradeRequest)
			{
				if (personRequest.Request is IShiftTradeRequest shiftTradeRequest)
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

		private IRequestApprovalService getRequestApprovalService(IPersonRequest personRequest,
			IScheduleDictionary scheduleDictionary, ApproveRequestCommand command)
		{
			var requestType = personRequest.Request.RequestType;
			switch (requestType)
			{
				case RequestType.AbsenceRequest:
					return _requestApprovalServiceFactory.MakeAbsenceRequestApprovalService(scheduleDictionary,
						_currentScenario.Current(), personRequest.Person);
				case RequestType.ShiftTradeRequest:
					return _requestApprovalServiceFactory.MakeShiftTradeRequestApprovalService(scheduleDictionary,
						personRequest.Person);
				case RequestType.OvertimeRequest:
					return _requestApprovalServiceFactory.MakeOvertimeRequestApprovalService(command.OvertimeValidatedSkillDictionary);
			}
			return null;
		}
	}
}