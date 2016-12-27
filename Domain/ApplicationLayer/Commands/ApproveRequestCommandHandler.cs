﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
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
			if (personRequest.IsDeleted || (personRequest.IsDenied && !personRequest.IsWaitlisted) || personRequest.IsCancelled)
			{
				return invalidRequestState(personRequest, command);
			}

			var scheduleDictionary = getSchedules(personRequest);
			_requestApprovalService = _requestApprovalServiceFactory.MakeRequestApprovalServiceScheduler(scheduleDictionary,
				_currentScenario.Current(), personRequest.Person);

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
			var scheduleChangedWithBrokenRules = false;
			foreach (var range in scheduleDictionary.Values)
			{
				var diff = range.DifferenceSinceSnapshot(_differenceService);

				if (anyRuleBroken && diff.Any())
				{
					scheduleChangedWithBrokenRules = true;
					logger.Warn($"Total {brokenRuleResponses.Count} business rules broken on approving the request with Id=\"{personRequest.Id}\", "
								+ $"but the schedule from {range.Period.StartDateTime:yyyy-mm-dd} "
								+ $"to {range.Period.EndDateTime:yyyy-mm-dd} was changed.");
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
				command.ErrorMessages.Add(UserTexts.Resources.RequestHasBeenDeleted);
			}
			else
			{
				command.ErrorMessages.Add(string.Format(UserTexts.Resources.RequestInvalidStateTransition,personRequest.StatusText,
				UserTexts.Resources.Approved));
			}
			
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