using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade
{
	[InstancePerLifetimeScope]
	public class ShiftTradeRequestHandler :
		IHandleEvent<NewShiftTradeRequestCreatedEvent>,
		IHandleEvent<AcceptShiftTradeEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ShiftTradeRequestHandler));

		private static readonly ISpecification<IShiftTradeRequest> shouldShiftTradeBeAutoGranted =
			new ShouldShiftTradeBeAutoGrantedSpecification();

		private static readonly ISpecification<IPersonRequest> isRequestReadyForProcessing =
			new isRequestReadyForProcessingSpecification();

		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly IBusinessRuleProvider _businessRuleProvider;

		private readonly ILoadSchedulesForRequestWithoutResourceCalculation
			_loadSchedulingDataForRequestWithoutResourceCalculation;

		private readonly IPersonRepository _personRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IRequestFactory _requestFactory;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IScheduleStorage _scheduleStorage;

		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IShiftTradePendingReasonsService _shiftTradePendingReasonsService;
		private readonly IShiftTradeValidator _validator;
		private readonly IShiftTradeApproveService _shiftTradeApproveService;

		public ShiftTradeRequestHandler(
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IShiftTradeValidator validator,
			IRequestFactory requestFactory, ICurrentScenario scenarioRepository,
			IPersonRequestRepository personRequestRepository,
			IScheduleStorage scheduleStorage, IPersonRepository personRepository,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization,
			ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulingDataForRequestWithoutResourceCalculation,
			IBusinessRuleProvider businessRuleProvider,
			IShiftTradePendingReasonsService shiftTradePendingReasonsService, IShiftTradeApproveService shiftTradeApproveService)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_validator = validator;
			_authorization = personRequestCheckAuthorization;
			_requestFactory = requestFactory;
			_scenarioRepository = scenarioRepository;
			_personRequestRepository = personRequestRepository;
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_loadSchedulingDataForRequestWithoutResourceCalculation = loadSchedulingDataForRequestWithoutResourceCalculation;
			_businessRuleProvider = businessRuleProvider;
			_shiftTradePendingReasonsService = shiftTradePendingReasonsService;
			_shiftTradeApproveService = shiftTradeApproveService;

			logger.Info("New instance of Shift Trade saga was created");
		}

		[AsSystem, UnitOfWork]
		public virtual void Handle(NewShiftTradeRequestCreatedEvent @event)
		{
			logger.DebugFormat("Consuming @event for person request with Id = {0}. (@event timestamp = {1})",
							   @event.PersonRequestId, @event.Timestamp);
			var personRequest = loadPersonRequest(@event.PersonRequestId);
			if (!isRequestReadyForProcessing.IsSatisfiedBy(personRequest))
			{
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat(
						"No person request found with the supplied Id, or the request is not in New or Pending status mode. (Id = {0})",
						@event.PersonRequestId);
				}

				clearStateHolder();
				return;
			}
			var scenario = loadDefaultScenario();
			var shiftTradeRequest = getShiftTradeRequest(personRequest);

			loadSchedules(shiftTradeRequest.Period, shiftTradeRequest.InvolvedPeople(), scenario);
			var shiftTradeRequestStatusChecker = _requestFactory.GetShiftTradeRequestStatusChecker(_schedulingResultStateHolder);
			getShiftTradeStatus(shiftTradeRequestStatusChecker, shiftTradeRequest);
			var validationResult = validateRequest(shiftTradeRequest);
			setPersonRequestState(validationResult, personRequest, shiftTradeRequest);

			clearStateHolder();
		}

		[AsSystem, UnitOfWork]
		public virtual void Handle(AcceptShiftTradeEvent @event)
		{
			logger.DebugFormat("Consuming @event for person request with Id = {0}. (@event timestamp = {1})",
				@event.PersonRequestId, @event.Timestamp);

			logger.DebugFormat("Loading PersonRequest = {0}", @event.PersonRequestId);
			var personRequest = loadPersonRequest(@event.PersonRequestId);
			if (!isRequestReadyForProcessing.IsSatisfiedBy(personRequest))
			{
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat(
						"No person request found with the supplied Id, or the request is not in New or Pending status mode. (Id = {0})",
						@event.PersonRequestId);
				}
				clearStateHolder();
				return;
			}

			logger.Debug("Loading Default Scenario");
			var scenario = loadDefaultScenario();

			logger.Debug("Loading Schedules");
			var shiftTradeRequest = getShiftTradeRequest(personRequest);
			loadSchedules(shiftTradeRequest.Period, shiftTradeRequest.InvolvedPeople(), scenario);
			var shiftTradeRequestStatusChecker = _requestFactory.GetShiftTradeRequestStatusChecker(_schedulingResultStateHolder);

			logger.Debug("Checking MF ShiftTrade status");
			var shiftTradeStatus = getShiftTradeStatus(shiftTradeRequestStatusChecker, shiftTradeRequest);
			logger.DebugFormat("Status is: {0}", shiftTradeStatus);

			logger.Debug("Validating ShiftTrade");
			var validationResult = validateRequest(shiftTradeRequest);

			if (checkStatus(shiftTradeStatus, validationResult))
			{
				logger.Debug("Loading Accepting person");
				var acceptingPerson = loadPersonAcceptingPerson(@event);
				var checkSum = new ShiftTradeRequestSetChecksum(_scenarioRepository, _scheduleStorage);

				try
				{
					logger.DebugFormat("Accepting ShiftTrade: {0}", personRequest.GetSubject(new NormalizeText()));
					((IShiftTradeRequest)personRequest.Request).Accept(acceptingPerson, checkSum, _authorization);
					setUpdatedMessage(@event, personRequest);

					_schedulingResultStateHolder.UseMaximumWorkday = @event.UseMaximumWorkday;
					var allEnabledRules = getAllEnabledBusinessRules(personRequest.Person.PermissionInformation.UICulture(), @event);

					if (!validationResult.IsOk)
					{
						allEnabledRules.Add(new ShiftTradeValidationFailedRule(getValidationMessage(validationResult, personRequest),
							personRequest.Request.Period.ToDateOnlyPeriod(personRequest.Person.PermissionInformation.DefaultTimeZone()),
							validationResult.SpecificationType));
					}

					var approvalService = _requestFactory.GetRequestApprovalService(allEnabledRules, scenario,
						_schedulingResultStateHolder, personRequest);

					personRequest.Pending();

					var ruleResponses = new List<IBusinessRuleResponse>();
					if (shouldShiftTradeBeAutoGranted.IsSatisfiedBy(shiftTradeRequest))
					{
						logger.DebugFormat("Approving ShiftTrade: {0}", personRequest.GetSubject(new NormalizeText()));
						ruleResponses.AddRange(_shiftTradeApproveService.AutoApprove(personRequest, approvalService, _schedulingResultStateHolder));
						if (ruleResponses.Any())
						{
							var deniableResponse = _businessRuleProvider.GetFirstDeniableResponse(allEnabledRules, ruleResponses);
							if (deniableResponse != null)
							{
								personRequest.Deny(deniableResponse.Message, _authorization);
							}
						}
					}
					else
					{
						ruleResponses.AddRange(_shiftTradeApproveService.SimulateApprove(shiftTradeRequest, allEnabledRules,
							_schedulingResultStateHolder));
					}

					handleBrokenBusinessRules(ruleResponses, personRequest);
				}
				catch (ShiftTradeRequestStatusException exception)
				{
					logger.Error("An exception occured when trying to accept the shift trade request.", exception);
					clearStateHolder();
					return;
				}
				catch (ValidationException exception)
				{
					logger.Error("A validation exception occured when trying to accept the shift trade request.", exception);
					clearStateHolder();
					return;
				}

				var status = shiftTradeRequest.GetShiftTradeStatus(shiftTradeRequestStatusChecker);
				logger.InfoFormat("Shift trade state is Accepted, status is: {0}", status);
			}
			else if (validationResult.ShouldBeDenied)
			{
				personRequest.Deny(validationResult.DenyReason, _authorization);
			}

			clearStateHolder();
		}

		private static string getValidationMessage(ShiftTradeRequestValidationResult validationResult, IPersonRequest personRequest)
		{
			return Resources.ResourceManager.GetString(validationResult.DenyReason,
				personRequest.Person.PermissionInformation.UICulture());
		}

		private void setUpdatedMessage(AcceptShiftTradeEvent @event, IPersonRequest personRequest)
		{
			if (string.IsNullOrEmpty(@event.Message)) return;

			if (!personRequest.TrySetMessage(@event.Message))
			{
				logger.WarnFormat("Could not set @event to person request: {0}", @event.Message);
			}
		}

		private void handleBrokenBusinessRules(ICollection<IBusinessRuleResponse> brokenBusinessResponses,
			IPersonRequest personRequest)
		{
			_shiftTradePendingReasonsService.SetBrokenBusinessRulesFieldOnPersonRequest(brokenBusinessResponses, personRequest);

			if (brokenBusinessResponses.Count <= 0) return;

			var culture = personRequest.Person.PermissionInformation.UICulture();
			var sb = new StringBuilder(personRequest.GetMessage(new NormalizeText()));
			sb.AppendLine();
			sb.AppendLine($"{Resources.ResourceManager.GetString("ViolationOfABusinessRule", culture)}:");

			var brokenRuleMessages = brokenBusinessResponses.Select(m => m.Message).Distinct().ToArray();
			if (brokenRuleMessages.Any())
			{
				var index = 1;
				var sbBrokenRuleMessages = new StringBuilder();
				sbBrokenRuleMessages.AppendLine("The following @event is from broken rules on handle PersonRequest "
												+ $"with Id=\"{personRequest.Id}\":");
				foreach (var brokenBusinessRuleMessage in brokenRuleMessages)
				{
					sb.AppendLine(brokenBusinessRuleMessage);
					sbBrokenRuleMessages.AppendLine($"  {index}. {brokenBusinessRuleMessage}");
					index++;
				}

				if (logger.IsInfoEnabled)
				{
					logger.Info(sbBrokenRuleMessages.ToString());
				}
			}

			if (!personRequest.TrySetMessage(sb.ToString()))
			{
				logger.WarnFormat("Could not set @event with broken business rules to person request: {0}", sb);
			}
		}

		private void clearStateHolder()
		{
			_schedulingResultStateHolder.Schedules = null;
			_schedulingResultStateHolder.LoadedAgents = null;
		}

		private INewBusinessRuleCollection getAllEnabledBusinessRules(CultureInfo cultureInfo, AcceptShiftTradeEvent @event)
		{
			var rules = _businessRuleProvider.GetAllEnabledBusinessRulesForShiftTradeRequest(_schedulingResultStateHolder,
				@event.UseSiteOpenHoursRule);
			rules.SetUICulture(cultureInfo);

			return rules;
		}

		private ShiftTradeStatus getShiftTradeStatus(IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker,
			IShiftTradeRequest shiftTradeRequest)
		{
			return shiftTradeRequest.GetShiftTradeStatus(shiftTradeRequestStatusChecker);
		}

		private bool checkStatus(ShiftTradeStatus shiftTradeStatus, ShiftTradeRequestValidationResult validationResult)
		{
			return (shiftTradeStatus == ShiftTradeStatus.OkByMe || shiftTradeStatus == ShiftTradeStatus.OkByBothParts) && !validationResult.ShouldBeDenied;
		}

		private IPerson loadPersonAcceptingPerson(AcceptShiftTradeEvent @event)
		{
			return _personRepository.Get(@event.AcceptingPersonId);
		}

		private ShiftTradeRequestValidationResult validateRequest(IShiftTradeRequest shiftTradeRequest)
		{
			return _validator.Validate(shiftTradeRequest);
			//Logger.InfoFormat("Validated Shift Trade, State is Validated = {0}", _validationResult.Value);
		}

		private void setPersonRequestState(ShiftTradeRequestValidationResult validationResult, IPersonRequest personRequest,
										   IShiftTradeRequest shiftTradeRequest)
		{
			if (!validationResult.ShouldBeDenied)
			{
				personRequest.Pending();
				shiftTradeRequest.NotifyToPersonAfterValidation();
			}
			else
			{
				var involvedPeople = shiftTradeRequest.InvolvedPeople();
				//To avoid notifications to the second part in the trade that the trade was denied.
				var fakeDenier = involvedPeople.FirstOrDefault(p => !p.Equals(shiftTradeRequest.Person));
				personRequest.Deny(validationResult.DenyReason, _authorization, fakeDenier);
				logger.InfoFormat("Shift Trade is denied, Reason: {0}", validationResult.DenyReason);
			}
		}

		private IPersonRequest loadPersonRequest(Guid personRequestId)
		{
			return _personRequestRepository.Get(personRequestId);
		}

		private IShiftTradeRequest getShiftTradeRequest(IPersonRequest personRequest)
		{
			return (IShiftTradeRequest)personRequest.Request;
		}

		private IScenario loadDefaultScenario()
		{
			return _scenarioRepository.Current();
			//Logger.DebugFormat("Using the default scenario named {0}. (Id = {1})", _defaultScenario.Description, _defaultScenario.Id);
		}

		private void loadSchedules(DateTimePeriod period, IEnumerable<IPerson> persons, IScenario scenario)
		{
			_loadSchedulingDataForRequestWithoutResourceCalculation.Execute(scenario, period, persons.ToList(),
				_schedulingResultStateHolder);
		}

		private class isRequestReadyForProcessingSpecification : Specification<IPersonRequest>
		{
			public override bool IsSatisfiedBy(IPersonRequest personRequest)
			{
				return personRequest != null && (personRequest.IsNew || personRequest.IsPending);
			}
		}
	}
}