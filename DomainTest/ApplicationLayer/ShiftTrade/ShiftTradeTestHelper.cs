using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	class ShiftTradeTestHelper
	{
		private ShiftTradeRequestHandler _target;
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly FakeRequestFactory _requestFactory;
		private IShiftTradeValidator _validator;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IBusinessRuleProvider _businessRuleProvider;
		private readonly INewBusinessRuleCollection _businessRuleCollection;
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IShiftTradePendingReasonsService _shiftTradePendingReasonsService;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulingDataForRequestWithoutResourceCalculation;
		private readonly IScheduleProjectionReadOnlyActivityProvider _scheduleProjectionReadOnlyActivityProvider;
		private readonly IShiftTradeMaxSeatValidator _shiftTradeMaxSeatReadModelValidator;
		private readonly IShiftTradeMaxSeatValidator _shiftTradeMaxSeatValidator;
		private IShiftTradeMaxSeatValidator _activeShiftTradeMaxSeatValidator;

		public ShiftTradeTestHelper (ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleStorage scheduleStorage, IPersonRepository personRepository, IBusinessRuleProvider businessRuleProvider, INewBusinessRuleCollection businessRuleCollection, ICurrentScenario currentScenario, IScheduleProjectionReadOnlyActivityProvider scheduleProjectionReadOnlyActivityProvider)
		{
			_personRequestRepository = new FakePersonRequestRepository();
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_requestFactory = new FakeRequestFactory();

			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_businessRuleProvider = businessRuleProvider;
			_businessRuleCollection = businessRuleCollection;
			_currentScenario = currentScenario;
			_scheduleProjectionReadOnlyActivityProvider = scheduleProjectionReadOnlyActivityProvider;

			_scheduleDifferenceSaver = new ScheduleDifferenceSaver (_scheduleStorage, CurrentUnitOfWork.Make());
			;_shiftTradePendingReasonsService = new ShiftTradePendingReasonsService (_requestFactory, _currentScenario);

			_globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			_globalSettingDataRepository.PersistSettingValue (ShiftTradeSettings.SettingsKey, new ShiftTradeSettings()
			{
				MaxSeatsValidationEnabled = true,
				MaxSeatsValidationSegmentLength = 15
			});

			_shiftTradeMaxSeatReadModelValidator = new ShiftTradeMaxSeatReadModelValidator(_scheduleProjectionReadOnlyActivityProvider, _currentScenario);
			_shiftTradeMaxSeatValidator = new ShiftTradeMaxSeatValidator (_currentScenario, _scheduleStorage, _personRepository);
			_activeShiftTradeMaxSeatValidator = _shiftTradeMaxSeatValidator;

			var shiftTradeSpecifications = new List<IShiftTradeSpecification>
			{
				new ShiftTradeValidatorTest.ValidatorSpecificationForTest (true, "_openShiftTradePeriodSpecification"),
				new ShiftTradeMaxSeatsSpecification (_globalSettingDataRepository, _shiftTradeMaxSeatReadModelValidator)

			};
			_validator = new ShiftTradeValidator(new FakeShiftTradeLightValidator(), shiftTradeSpecifications);

			_loadSchedulingDataForRequestWithoutResourceCalculation =
				new LoadSchedulesForRequestWithoutResourceCalculation (new FakePersonAbsenceAccountRepository(), _scheduleStorage);
		}

		internal void UseMaxSeatReadModelValidator(bool enabled)
		{
			if (enabled)
				setValidator (_shiftTradeMaxSeatReadModelValidator);
			else
			{
				setValidator(_shiftTradeMaxSeatValidator);
			}
		}

		internal static WorkflowControlSet CreateWorkFlowControlSet(bool autoGrantShiftTrade)
		{
			var workflowControlSet = new WorkflowControlSet { AutoGrantShiftTradeRequest = autoGrantShiftTrade }.WithId();
			return workflowControlSet;

		}

		internal void OverrideShiftTradeGlobalSettings(ShiftTradeSettings shiftTradeSettings)
		{
			_globalSettingDataRepository.PersistSettingValue(ShiftTradeSettings.SettingsKey, shiftTradeSettings);

			setValidator(_activeShiftTradeMaxSeatValidator);
		}

		private void setValidator(IShiftTradeMaxSeatValidator shiftTradeMaxSeatValidator)
		{
			var shiftTradeSpecifications = new List<IShiftTradeSpecification>
			{
				new ShiftTradeValidatorTest.ValidatorSpecificationForTest (true, "_openShiftTradePeriodSpecification"),
				new ShiftTradeMaxSeatsSpecification (_globalSettingDataRepository, shiftTradeMaxSeatValidator)
			};

			_validator = new ShiftTradeValidator (new FakeShiftTradeLightValidator(), shiftTradeSpecifications);
			_activeShiftTradeMaxSeatValidator = shiftTradeMaxSeatValidator;
		}


		internal IPerson CreatePersonInTeam(ITeam team)
		{
			var workControlSet = CreateWorkFlowControlSet(true);
			var startDate = new DateOnly(2016, 1, 1);
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, team);
			person.WorkflowControlSet = workControlSet;
			_personRepository.Add(person);

			return person;
		}


		internal IPersonRequest PrepareAndExecuteRequest(IPerson personTo, IPerson personFrom, DateOnly scheduleDateOnly, IPerson[] allPeople, DateTime scheduleDate)
		{
			SetPersonAccounts(personTo, personFrom, scheduleDateOnly);
			var personRequest = PrepareAndGetPersonRequest(personFrom, personTo, scheduleDateOnly);

			var @event = GetAcceptShiftTradeEvent(personTo, personRequest.Id.GetValueOrDefault());
			@event.UseSiteOpenHoursRule = true;

			var businessRuleProvider = new BusinessRuleProvider();
			var scheduleDictionary =
				_scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(allPeople, null,
					new DateOnlyPeriod(new DateOnly(scheduleDate), new DateOnly(scheduleDate.AddDays(7))), _currentScenario.Current());

			SetApprovalService(scheduleDictionary, businessRuleProvider.GetAllBusinessRules(_schedulingResultStateHolder));

			HandleRequest(@event, false, businessRuleProvider);
			return personRequest;
		}

		internal void SetPersonAccounts(IPerson personTo, IPerson personFrom, DateOnly scheduleDateOnly)
		{
			_schedulingResultStateHolder.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>
			{
				{personTo, new PersonAccountCollection(personTo) {CreatePersonAbsenceAccount(personTo, scheduleDateOnly)}},
				{personFrom, new PersonAccountCollection(personFrom) {CreatePersonAbsenceAccount(personFrom, scheduleDateOnly)}}
			};
		}

		internal AcceptShiftTradeEvent GetAcceptShiftTradeEvent(IPerson personTo, Guid requestId)
		{
			var ast = new AcceptShiftTradeEvent
			{
				LogOnDatasource = "V7Config",
				LogOnBusinessUnitId = Guid.NewGuid(),
				Timestamp = DateTime.UtcNow,
				PersonRequestId = requestId,
				Message = "I want to trade!",
				AcceptingPersonId = personTo.Id.GetValueOrDefault()
			};
			return ast;
		}

		internal IPersonRequest PrepareAndGetPersonRequest(IPerson personFrom, IPerson personTo, DateOnly shiftTradeDate)
		{
			var personRequest = new PersonRequestFactory().CreatePersonShiftTradeRequest(personFrom, personTo, shiftTradeDate).WithId();

			new ShiftTradeSwapScheduleDetailsMapper(_scheduleStorage, _currentScenario).Map((ShiftTradeRequest)personRequest.Request);

			_personRequestRepository.Add(personRequest);
			return personRequest;
		}

		internal void SetApprovalService(IScheduleDictionary scheduleDictionary, INewBusinessRuleCollection newBusinessRules = null)
		{
			var approvalService = new RequestApprovalServiceScheduler(scheduleDictionary, _currentScenario.Current(),
				new SwapAndModifyService(new SwapService(), new DoNothingScheduleDayChangeCallBack()), newBusinessRules ?? _businessRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new FakeGlobalSettingDataRepository(), null);

			_requestFactory.setRequestApprovalService(approvalService);
		}

		internal void HandleRequest(AcceptShiftTradeEvent acceptShiftTradeEvent, bool toggle39473IsOff = false, IBusinessRuleProvider businessRuleProvider = null)
		{

			_target = new ShiftTradeRequestHandler(_schedulingResultStateHolder, _validator, _requestFactory,
				_currentScenario, _personRequestRepository, _scheduleStorage, _personRepository
				, new PersonRequestAuthorizationCheckerForTest(), _scheduleDifferenceSaver,
				_loadSchedulingDataForRequestWithoutResourceCalculation,
				new DifferenceEntityCollectionService<IPersistableScheduleData>(),
				businessRuleProvider ?? _businessRuleProvider,
				toggle39473IsOff ? new ShiftTradePendingReasonsService39473ToggleOff() : _shiftTradePendingReasonsService);
			_target.Handle(acceptShiftTradeEvent);
		}


		internal PersonAbsenceAccount CreatePersonAbsenceAccount(IPerson person, DateOnly scheduleDateOnly)
		{
			var accountDay = new AccountDay(scheduleDateOnly)
			{
				BalanceIn = TimeSpan.FromDays(0),
				Accrued = TimeSpan.FromDays(0),
				Extra = TimeSpan.FromDays(0)
			};
			var personFromAbsenceAccount = new PersonAbsenceAccount(person, AbsenceFactory.CreateAbsence("Holiday"));
			personFromAbsenceAccount.Absence.Tracker = Tracker.CreateDayTracker();
			personFromAbsenceAccount.Add(accountDay);
			return personFromAbsenceAccount;
		}

		internal IPersonAssignment AddPersonAssignment(IPerson person, DateTimePeriod dateTimePeriod, IActivity activity = null)
		{
			var scenario = _currentScenario.Current();
			var personAssignment = activity != null ?
				PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, dateTimePeriod, new ShiftCategory("AM"), scenario).WithId() :
				PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, dateTimePeriod).WithId();
			_scheduleStorage.Add(personAssignment);
			return personAssignment;
		}
	}

	class ActivityAndDateTime
	{
		public DateTimePeriod Period { get; set; }
		public IActivity Activity { get; set; }
	}
}