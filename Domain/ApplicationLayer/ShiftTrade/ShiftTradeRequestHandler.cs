using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade
{
	public class ShiftTradeRequestHandler
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(ShiftTradeRequestHandler));

		private static readonly ISpecification<IShiftTradeRequest> shouldShiftTradeBeAutoGranted =
			new ShouldShiftTradeBeAutoGrantedSpecification();

		private static readonly ISpecification<IPersonRequest> isRequestReadyForProcessing =
			new IsRequestReadyForProcessingSpecification();

		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly IBusinessRuleProvider _businessRuleProvider;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceService;

		private readonly ILoadSchedulesForRequestWithoutResourceCalculation
			_loadSchedulingDataForRequestWithoutResourceCalculation;

		private readonly IMessageBrokerComposite _messageBroker;
		private readonly IPersonRepository _personRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IRequestFactory _requestFactory;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;
		private readonly IScheduleStorage _scheduleStorage;

		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IShiftTradePendingReasonsService _shiftTradePendingReasonsService;
		private readonly IShiftTradeValidator _validator;

		public ShiftTradeRequestHandler(
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IShiftTradeValidator validator,
			IRequestFactory requestFactory, ICurrentScenario scenarioRepository,
			IPersonRequestRepository personRequestRepository,
			IScheduleStorage scheduleStorage, IPersonRepository personRepository,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization,
			IScheduleDifferenceSaver scheduleDictionarySaver,
			ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulingDataForRequestWithoutResourceCalculation,
			IDifferenceCollectionService<IPersistableScheduleData> differenceService,
			IMessageBrokerComposite messageBroker, IBusinessRuleProvider businessRuleProvider,
			IShiftTradePendingReasonsService shiftTradePendingReasonsService)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_validator = validator;
			_authorization = personRequestCheckAuthorization;
			_requestFactory = requestFactory;
			_scenarioRepository = scenarioRepository;
			_personRequestRepository = personRequestRepository;
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_scheduleDictionarySaver = scheduleDictionarySaver;
			_loadSchedulingDataForRequestWithoutResourceCalculation = loadSchedulingDataForRequestWithoutResourceCalculation;
			_differenceService = differenceService;
			_messageBroker = messageBroker;
			_businessRuleProvider = businessRuleProvider;
			_shiftTradePendingReasonsService = shiftTradePendingReasonsService;

			logger.Info("New instance of Shift Trade saga was created");
		}

		public void Handle(NewShiftTradeRequestCreatedEvent @event)
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

		public void Handle(ProjectionChangedEvent @event)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Handle ProjectionChangedEvent with PersonId=\"{0}\" and total {1} schedule days",
					@event.PersonId, @event.ScheduleDays.Count);
			}

			var person = _personRepository.Get(@event.PersonId);
			var scheduleDays = @event.ScheduleDays;
			if (person == null || !scheduleDays.Any()) return;

			var scenario = loadDefaultScenario();
			var allAffectedRequests = new Dictionary<IPersonRequest, IList<DateTime>>();
			foreach (var scheduleDay in scheduleDays)
			{
				var period = getScheduleDayPeriod(scheduleDay);
				var personRequests = _personRequestRepository.FindAllRequestsForAgent(person, period).ToList();
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0} shift trade requests found for person with Id=\"{1}\" contains period "
									   + "from \"{2:yyyy-MM-dd HH:mm:ss}\" to \"{3:yyyy-MM-dd HH:mm:ss}\" "
									   + "(ScheduleDay: {4:yyyy-MM-dd})", personRequests.Count, person.Id,
						period.StartDateTime, period.EndDateTime, scheduleDay.Date);
				}
				foreach (var personRequest in personRequests)
				{
					if (personRequest.Id == null) continue;
					if (!allAffectedRequests.ContainsKey(personRequest))
					{
						allAffectedRequests.Add(personRequest, new List<DateTime>
						{
							scheduleDay.Date
						});
					}
					else
					{
						allAffectedRequests[personRequest].Add(scheduleDay.Date);
					}
				}
			}
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Total {0} affected shift trade requests found for person with Id=\"{1}\"",
					allAffectedRequests.Count, person.Id);
			}

			foreach (var requestAndDate in allAffectedRequests)
			{
				var personRequest = requestAndDate.Key;
				var shiftTradeRequest = getShiftTradeRequest(personRequest);
				var previousStatus = shiftTradeRequest.GetShiftTradeStatus(new EmptyShiftTradeRequestChecker());
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Current status of person request with Id=\"{0}\" is {1}", personRequest.Id, previousStatus);
				}
				if (previousStatus != ShiftTradeStatus.OkByBothParts) continue;

				loadSchedules(shiftTradeRequest.Period, shiftTradeRequest.InvolvedPeople(), scenario);
				var shiftTradeRequestStatusChecker
					= _requestFactory.GetShiftTradeRequestStatusChecker(_schedulingResultStateHolder);
				var newShiftTradeStatus = shiftTradeRequest.GetShiftTradeStatus(shiftTradeRequestStatusChecker);
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("New status after check of person request with Id=\"{0}\" is {1}", personRequest.Id,
						newShiftTradeStatus);
				}
				if (newShiftTradeStatus != ShiftTradeStatus.Referred) continue;

				var theOtherPerson = shiftTradeRequest.InvolvedPeople().FirstOrDefault(p => !p.Equals(person));
				if (theOtherPerson == null) continue;

				var theOtherPersonId = theOtherPerson.Id.GetValueOrDefault();
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Status changed, will send notification to person with Id=\"{0}\"", theOtherPersonId);
				}

				var allDates = requestAndDate.Value;
				sendNotification(@event, theOtherPersonId, allDates.Min(), allDates.Max());
			}
		}

		private DateTimePeriod getScheduleDayPeriod(ProjectionChangedEventScheduleDay scheduleDay)
		{
			DateTime startTimeUtc, endTimeUtc;

			if (scheduleDay.Shift != null)
			{
				startTimeUtc = scheduleDay.Shift.StartDateTime.ToUniversalTime();
				endTimeUtc = scheduleDay.Shift.EndDateTime.ToUniversalTime();
			}
			else if (scheduleDay.DayOff != null)
			{
				startTimeUtc = scheduleDay.DayOff.StartDateTime.ToUniversalTime();
				endTimeUtc = scheduleDay.DayOff.EndDateTime.ToUniversalTime();
			}
			else
			{
				startTimeUtc = scheduleDay.Date.ToUniversalTime();
				endTimeUtc = startTimeUtc.AddDays(1);
			}
			return new DateTimePeriod(startTimeUtc, endTimeUtc);
		}

		private void sendNotification(ProjectionChangedEvent @event, Guid personId, DateTime startDate, DateTime endDate)
		{
			_messageBroker.Send(
				@event.LogOnDatasource,
				@event.LogOnBusinessUnitId,
				startDate,
				endDate,
				Guid.Empty,
				personId,
				typeof(Person),
				Guid.Empty,
				typeof(IShiftTradeScheduleChangedInDefaultScenario),
				DomainUpdateType.NotApplicable,
				null,
				@event.CommandId == Guid.Empty ? Guid.NewGuid() : @event.CommandId);
		}

		public void Handle(AcceptShiftTradeEvent @event)
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
					personRequest.Request.Accept(acceptingPerson, checkSum, _authorization);
					setUpdatedMessage(@event, personRequest);

					_schedulingResultStateHolder.UseMinWeekWorkTime = @event.UseMinWeekWorkTime;
					var allNewRules = getAllNewBusinessRules(personRequest.Person.PermissionInformation.UICulture());
					var approvalService = _requestFactory.GetRequestApprovalService(allNewRules, scenario, _schedulingResultStateHolder);

					personRequest.Pending();

					if (shouldShiftTradeBeAutoGranted.IsSatisfiedBy(shiftTradeRequest))
					{
						var brokenBusinessRules = autoApproveShiftTrade(personRequest, approvalService);
						_shiftTradePendingReasonsService.SetBrokenBusinessRulesFieldOnPersonRequest(brokenBusinessRules, personRequest);
					}
					else
					{
						_shiftTradePendingReasonsService.SimulateApproveAndSetBusinessRuleResponsesOnFail(shiftTradeRequest, allNewRules,
							_schedulingResultStateHolder);
					}
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
			else if (!validationResult.Value)
			{
				personRequest.Deny(null, validationResult.DenyReason, _authorization);
			}

			clearStateHolder();
		}

		private IList<IBusinessRuleResponse> autoApproveShiftTrade(IPersonRequest personRequest,
			IRequestApprovalService approvalService)
		{
			logger.DebugFormat("Approving ShiftTrade: {0}", personRequest.GetSubject(new NormalizeText()));
			var brokenBusinessRules = personRequest.Approve(approvalService, _authorization, true);

			handleBrokenBusinessRules(brokenBusinessRules, personRequest);
			foreach (var range in _schedulingResultStateHolder.Schedules.Values)
			{
				var diff = range.DifferenceSinceSnapshot(_differenceService);
				_scheduleDictionarySaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate)range);
			}
			return brokenBusinessRules;
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
			if (brokenBusinessResponses.Count <= 0) return;

			var culture = personRequest.Person.PermissionInformation.UICulture();
			var sb = new StringBuilder(personRequest.GetMessage(new NormalizeText()));
			sb.AppendLine();
			sb.Append(UserTexts.Resources.ResourceManager.GetString("ViolationOfABusinessRule",
				culture)).Append(":").AppendLine();
			foreach (var brokenBusinessRuleMessage in brokenBusinessResponses.Select(m => m.Message).Distinct())
			{
				sb.AppendLine(brokenBusinessRuleMessage);
				if (logger.IsWarnEnabled)
				{
					logger.WarnFormat("The following @event is from a broken rule: {0}",
						brokenBusinessRuleMessage);
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
			_schedulingResultStateHolder.PersonsInOrganization = null;
		}

		private INewBusinessRuleCollection getAllNewBusinessRules(CultureInfo cultureInfo)
		{
			var rules = _businessRuleProvider.GetAllBusinessRules(_schedulingResultStateHolder);
			rules.Remove(typeof(NewPersonAccountRule));
			rules.Remove(typeof(OpenHoursRule));
			rules.Add(new NonMainShiftActivityRule());
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
			return shiftTradeStatus == ShiftTradeStatus.OkByMe && validationResult.Value;
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
			if (validationResult.Value)
			{
				personRequest.Pending();
				shiftTradeRequest.NotifyToPersonAfterValidation();
			}
			else
			{
				var involvedPeople = shiftTradeRequest.InvolvedPeople();
				//To avoid notifications to the second part in the trade that the trade was denied.
				var fakeDenier = involvedPeople.FirstOrDefault(p => !p.Equals(shiftTradeRequest.Person));
				personRequest.Deny(fakeDenier, validationResult.DenyReason, _authorization);
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

		private class IsRequestReadyForProcessingSpecification : Specification<IPersonRequest>
		{
			public override bool IsSatisfiedBy(IPersonRequest obj)
			{
				return (obj != null && (obj.IsNew || obj.IsPending));
			}
		}
	}

	[EnabledBy(Toggles.ShiftTrade_ToHangfire_38181)]
	public class ShiftTradeRequestHandlerHangfire : ShiftTradeRequestHandler,
		IHandleEvent<NewShiftTradeRequestCreatedEvent>, IHandleEvent<AcceptShiftTradeEvent>,
		IHandleEvent<ProjectionChangedEvent>, IRunOnHangfire
	{
		public ShiftTradeRequestHandlerHangfire(
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IShiftTradeValidator validator,
			IRequestFactory requestFactory, ICurrentScenario scenarioRepository, IPersonRequestRepository personRequestRepository,
			IScheduleStorage scheduleStorage, IPersonRepository personRepository,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization, IScheduleDifferenceSaver scheduleDictionarySaver,
			ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulingDataForRequestWithoutResourceCalculation,
			IDifferenceCollectionService<IPersistableScheduleData> differenceService, IMessageBrokerComposite messageBroker,
			IBusinessRuleProvider businessRuleProvider, IShiftTradePendingReasonsService shiftTradePendingReasonsService)
			: base(schedulingResultStateHolder, validator, requestFactory, scenarioRepository,
				personRequestRepository, scheduleStorage, personRepository, personRequestCheckAuthorization, scheduleDictionarySaver,
				loadSchedulingDataForRequestWithoutResourceCalculation, differenceService, messageBroker, businessRuleProvider,
				shiftTradePendingReasonsService)
		{
		}

		[AsSystem, UnitOfWork]
		public new virtual void Handle(AcceptShiftTradeEvent @event)
		{
			base.Handle(@event);
		}

		[AsSystem, UnitOfWork]
		public new virtual void Handle(NewShiftTradeRequestCreatedEvent @event)
		{
			base.Handle(@event);
		}

		[AsSystem, UnitOfWork]
		public new virtual void Handle(ProjectionChangedEvent @event)
		{
			base.Handle(@event);
		}
	}

	[DisabledBy(Toggles.ShiftTrade_ToHangfire_38181)]
#pragma warning disable 618
	public class ShiftTradeRequestHandlerBus : ShiftTradeRequestHandler, IHandleEvent<NewShiftTradeRequestCreatedEvent>,
		IHandleEvent<AcceptShiftTradeEvent>, IHandleEvent<ProjectionChangedEvent>, IRunOnServiceBus
#pragma warning restore 618
	{
		public ShiftTradeRequestHandlerBus(
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IShiftTradeValidator validator,
			IRequestFactory requestFactory, ICurrentScenario scenarioRepository, IPersonRequestRepository personRequestRepository,
			IScheduleStorage scheduleStorage, IPersonRepository personRepository,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization, IScheduleDifferenceSaver scheduleDictionarySaver,
			ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulingDataForRequestWithoutResourceCalculation,
			IDifferenceCollectionService<IPersistableScheduleData> differenceService, IMessageBrokerComposite messageBroker,
			IBusinessRuleProvider businessRuleProvider, IShiftTradePendingReasonsService shiftTradePendingReasonsService)
			: base(schedulingResultStateHolder, validator, requestFactory, scenarioRepository,
				personRequestRepository, scheduleStorage, personRepository, personRequestCheckAuthorization, scheduleDictionarySaver,
				loadSchedulingDataForRequestWithoutResourceCalculation, differenceService, messageBroker, businessRuleProvider,
				shiftTradePendingReasonsService)
		{
		}
	}
}