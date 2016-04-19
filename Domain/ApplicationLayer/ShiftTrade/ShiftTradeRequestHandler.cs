using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade
{
	public class ShiftTradeRequestHandler : IHandleEvent<NewShiftTradeRequestCreatedEvent> , IHandleEvent<AcceptShiftTradeEvent>, IRunOnServiceBus
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ShiftTradeRequestHandler));

		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		//private readonly ISchedulingResultStateHolderProvider _schedulingResultStateHolderProvider;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IShiftTradeValidator _validator;
		private readonly IRequestFactory _requestFactory;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleDifferenceSaver _scheduleDictionarySaver;
		private readonly ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulingDataForRequestWithoutResourceCalculation;
		private readonly IDifferenceCollectionService<IPersistableScheduleData> _differenceService;
		private IPersonRequest _personRequest;
		private ShiftTradeRequestValidationResult _validationResult;
		private IShiftTradeRequest _shiftTradeRequest;

		private static readonly ISpecification<IShiftTradeRequest> ShouldShiftTradeBeAutoGranted =
			 new ShouldShiftTradeBeAutoGrantedSpecification();

		private static readonly ISpecification<IPersonRequest> IsRequestReadyForProcessing =
			 new IsRequestReadyForProcessingSpecification();

		private IPersonRequestCheckAuthorization _authorization;
		private IScenario _defaultScenario;

		public ShiftTradeRequestHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory,
			//ISchedulingResultStateHolderProvider schedulingResultStateHolderProvider, 
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IShiftTradeValidator validator,
			IRequestFactory requestFactory, ICurrentScenario scenarioRepository, IPersonRequestRepository personRequestRepository,
			IScheduleStorage scheduleStorage, IPersonRepository personRepository,
			IPersonRequestCheckAuthorization personRequestCheckAuthorization, IScheduleDifferenceSaver scheduleDictionarySaver,
			ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulingDataForRequestWithoutResourceCalculation,
			IDifferenceCollectionService<IPersistableScheduleData> differenceService)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			//_schedulingResultStateHolderProvider = schedulingResultStateHolderProvider;
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

			Logger.Info("New instance of Shift Trade saga was created");
		}

		public void Handle(NewShiftTradeRequestCreatedEvent @event)
		{
			//_schedulingResultStateHolder = _schedulingResultStateHolderProvider.GiveMeANew();
			Logger.DebugFormat("Consuming @event for person request with Id = {0}. (@event timestamp = {1})", @event.PersonRequestId, @event.Timestamp);
			using (IUnitOfWork unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				loadPersonRequest(@event.PersonRequestId);
				if (!IsRequestReadyForProcessing.IsSatisfiedBy(_personRequest))
				{
					if (Logger.IsWarnEnabled)
					{
						Logger.WarnFormat(
							 "No person request found with the supplied Id, or the request is not in New or Pending status mode. (Id = {0})",
							 @event.PersonRequestId);
					}

					clearStateHolder();
					return;
				}
				loadDefaultScenario();
				loadSchedules(_shiftTradeRequest.Period, _shiftTradeRequest.InvolvedPeople());
				var shiftTradeRequestStatusChecker = _requestFactory.GetShiftTradeRequestStatusChecker(_schedulingResultStateHolder);
				getShiftTradeStatus(shiftTradeRequestStatusChecker);
				validateRequest();
				setPersonRequestState();
				save(unitOfWork);
			}

			clearStateHolder();
		}

		public void Handle(AcceptShiftTradeEvent @event)
		{
			//_schedulingResultStateHolder = _schedulingResultStateHolderProvider.GiveMeANew();
			Logger.DebugFormat("Consuming @event for person request with Id = {0}. (@event timestamp = {1})", @event.PersonRequestId, @event.Timestamp);
			using (IUnitOfWork unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				Logger.DebugFormat("Loading PersonRequest = {0}", @event.PersonRequestId);
				loadPersonRequest(@event.PersonRequestId);
				if (!IsRequestReadyForProcessing.IsSatisfiedBy(_personRequest))
				{
					if (Logger.IsWarnEnabled)
					{
						Logger.WarnFormat(
							 "No person request found with the supplied Id, or the request is not in New or Pending status mode. (Id = {0})",
							 @event.PersonRequestId);
					}
					clearStateHolder();
					return;
				}
				Logger.Debug("Loading Default Scenario");
				loadDefaultScenario();
				Logger.Debug("Loading Schedules");
				loadSchedules(_shiftTradeRequest.Period, _shiftTradeRequest.InvolvedPeople());
				var shiftTradeRequestStatusChecker = _requestFactory.GetShiftTradeRequestStatusChecker(_schedulingResultStateHolder);

				Logger.Debug("Checking MF ShiftTrade status");
				ShiftTradeStatus shiftTradeStatus = getShiftTradeStatus(shiftTradeRequestStatusChecker);
				Logger.DebugFormat("Status is: {0}", shiftTradeStatus);

				Logger.Debug("Validating ShiftTrade");
				validateRequest();

				if (checkStatus(shiftTradeStatus))
				{
					Logger.Debug("Loading Accepting person");
					var acceptingPerson = loadPersonAcceptingPerson(@event);
					var checkSum = new ShiftTradeRequestSetChecksum(_scenarioRepository, _scheduleStorage);

					try
					{
						Logger.DebugFormat("Accepting ShiftTrade: {0}", _personRequest.GetSubject(new NormalizeText()));
						_personRequest.Request.Accept(acceptingPerson, checkSum, _authorization);
						setUpdatedMessage(@event);

						INewBusinessRuleCollection allNewRules = getAllNewBusinessRules();
						var approvalService = _requestFactory.GetRequestApprovalService(allNewRules, _defaultScenario,
							_schedulingResultStateHolder);

						_personRequest.Pending();
						if (ShouldShiftTradeBeAutoGranted.IsSatisfiedBy(_shiftTradeRequest))
						{
							Logger.DebugFormat("Approving ShiftTrade: {0}", _personRequest.GetSubject(new NormalizeText()));
							var brokenBusinessRules = _personRequest.Approve(approvalService, _authorization, true);
							handleBrokenBusinessRules(brokenBusinessRules);
							foreach (var range in _schedulingResultStateHolder.Schedules.Values)
							{
								var diff = range.DifferenceSinceSnapshot(_differenceService);
								_scheduleDictionarySaver.SaveChanges(diff, (IUnvalidatedScheduleRangeUpdate)range);
							}
						}
					}
					catch (ShiftTradeRequestStatusException exception)
					{
						Logger.Error("An exception occured when trying to accept the shift trade request.", exception);
						clearStateHolder();
						return;
					}
					catch (ValidationException exception)
					{
						Logger.Error("A validation exception occured when trying to accept the shift trade request.", exception);
						clearStateHolder();
						return;
					}

					var status = _shiftTradeRequest.GetShiftTradeStatus(shiftTradeRequestStatusChecker);
					Logger.InfoFormat("Shift trade state is Accepted, status is: {0}", status);
				}
				else if (!_validationResult.Value)
				{
					_personRequest.Deny(null, _validationResult.DenyReason, _authorization);
				}
				save(unitOfWork);
			}

			clearStateHolder();
		}

		private void setUpdatedMessage(AcceptShiftTradeEvent @event)
		{
			if (!string.IsNullOrEmpty(@event.Message))
			{
				if (!_personRequest.TrySetMessage(@event.Message))
				{
					Logger.WarnFormat("Could not set @event to person request: {0}", @event.Message);
				}
			}
		}

		private void handleBrokenBusinessRules(IList<IBusinessRuleResponse> brokenBusinessRules)
		{
			if (brokenBusinessRules.Count > 0)
			{
				var culture = _personRequest.Person.PermissionInformation.UICulture();

				StringBuilder sb = new StringBuilder(_personRequest.GetMessage(new NormalizeText()));
				sb.AppendLine();
				sb.Append(UserTexts.Resources.ResourceManager.GetString("ViolationOfABusinessRule",
																						  culture)).Append(":").AppendLine();
				foreach (var brokenBusinessRuleMessage in brokenBusinessRules.Select(m => m.Message).Distinct())
				{
					sb.AppendLine(brokenBusinessRuleMessage);
					if (Logger.IsWarnEnabled)
					{
						Logger.WarnFormat("The following @event is from a broken rule: {0}",
										  brokenBusinessRuleMessage);
					}
				}

				if (!_personRequest.TrySetMessage(sb.ToString()))
				{
					Logger.WarnFormat("Could not set @event with broken business rules to person request: {0}", sb);
				}
			}
		}

		private void clearStateHolder()
		{
			//_schedulingResultStateHolder.Dispose();
			//_schedulingResultStateHolder = null;
			_schedulingResultStateHolder.Schedules = null;
			_schedulingResultStateHolder.PersonsInOrganization = null;
			//_validator = null;
			//_authorization = null;
		}

		private INewBusinessRuleCollection getAllNewBusinessRules()
		{
			var rules = NewBusinessRuleCollection.All(_schedulingResultStateHolder);
			rules.Remove(typeof(NewPersonAccountRule));
			rules.Remove(typeof(OpenHoursRule));
			rules.Add(new NonMainShiftActivityRule());
			rules.SetUICulture(_personRequest.Person.PermissionInformation.UICulture());
			return rules;
		}

		private ShiftTradeStatus getShiftTradeStatus(IShiftTradeRequestStatusChecker shiftTradeRequestStatusChecker)
		{
			return _shiftTradeRequest.GetShiftTradeStatus(shiftTradeRequestStatusChecker);
		}

		private bool checkStatus(ShiftTradeStatus shiftTradeStatus)
		{
			return shiftTradeStatus == ShiftTradeStatus.OkByMe && _validationResult.Value;
		}

		private IPerson loadPersonAcceptingPerson(AcceptShiftTradeEvent @event)
		{
			return _personRepository.Get(@event.AcceptingPersonId);
		}

		private void validateRequest()
		{
			_validationResult = _validator.Validate(_shiftTradeRequest);
			Logger.InfoFormat("Validated Shift Trade, State is Validated = {0}", _validationResult.Value);
		}

		private void setPersonRequestState()
		{
			if (_validationResult.Value)
			{
				_personRequest.Pending();
				_shiftTradeRequest.NotifyToPersonAfterValidation();
			}
			else
			{
				var involvedPeople = _shiftTradeRequest.InvolvedPeople();
				//To avoid notifications to the second part in the trade that the trade was denied.
				var fakeDenier = involvedPeople.FirstOrDefault(p => !p.Equals(_shiftTradeRequest.Person));
				_personRequest.Deny(fakeDenier, _validationResult.DenyReason, _authorization);
				Logger.InfoFormat("Shift Trade is denied, Reason: {0}", _validationResult.DenyReason);
			}
		}

		private void loadPersonRequest(Guid personId)
		{
			_personRequest = _personRequestRepository.Get(personId);
			_shiftTradeRequest = (IShiftTradeRequest)_personRequest.Request;
		}

		private void loadDefaultScenario()
		{
			_defaultScenario = _scenarioRepository.Current();
			Logger.DebugFormat("Using the default scenario named {0}. (Id = {1})", _defaultScenario.Description, _defaultScenario.Id);
		}

		private static void save(IUnitOfWork unitOfWork)
		{
			unitOfWork.PersistAll();
		}

		private void loadSchedules(DateTimePeriod period, IEnumerable<IPerson> persons)
		{
			_loadSchedulingDataForRequestWithoutResourceCalculation.Execute(_defaultScenario, period, persons.ToList(), _schedulingResultStateHolder);
		}

		private class IsRequestReadyForProcessingSpecification : Specification<IPersonRequest>
		{
			public override bool IsSatisfiedBy(IPersonRequest obj)
			{
				return (obj != null && (obj.IsNew || obj.IsPending));
			}
		}
	}
}
