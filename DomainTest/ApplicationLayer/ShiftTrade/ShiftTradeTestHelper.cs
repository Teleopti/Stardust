using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Staffing;
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

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	internal class ShiftTradeTestHelper
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
		private readonly IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private readonly IShiftTradePendingReasonsService _shiftTradePendingReasonsService;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;
		private readonly ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulingDataForRequestWithoutResourceCalculation;
		private readonly IShiftTradeMaxSeatValidator _shiftTradeMaxSeatReadModelValidator;
		private IShiftTradeMaxSeatValidator _activeShiftTradeMaxSeatValidator;

		public ShiftTradeTestHelper(ISchedulingResultStateHolder schedulingResultStateHolder, IScheduleStorage scheduleStorage, IPersonRepository personRepository, IBusinessRuleProvider businessRuleProvider, ICurrentScenario currentScenario, IScheduleProjectionReadOnlyActivityProvider scheduleProjectionReadOnlyActivityProvider)
		{
			_personRequestRepository = new FakePersonRequestRepository();
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_requestFactory = new FakeRequestFactory();

			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_businessRuleProvider = businessRuleProvider;
			_currentScenario = currentScenario;

			_scheduleDifferenceSaver = new ScheduleDifferenceSaver(_scheduleStorage, CurrentUnitOfWork.Make(), new EmptyScheduleDayDifferenceSaver());
			_shiftTradePendingReasonsService = new ShiftTradePendingReasonsService();

			_globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			_globalSettingDataRepository.PersistSettingValue(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings()
			{
				MaxSeatsValidationEnabled = true,
				MaxSeatsValidationSegmentLength = 15
			});

			_shiftTradeMaxSeatReadModelValidator = new ShiftTradeMaxSeatReadModelValidator(scheduleProjectionReadOnlyActivityProvider, _currentScenario);

			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			globalSettingDataRepository.PersistSettingValue(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings
			{
				BusinessRuleConfigs = new ShiftTradeBusinessRuleConfig[] { }
			});

			var specificationChecker = new SpecificationCheckerWithConfig(GetDefaultShiftTradeSpecifications(), globalSettingDataRepository);

			_validator = new ShiftTradeValidator(new FakeShiftTradeLightValidator(), specificationChecker);
			_loadSchedulingDataForRequestWithoutResourceCalculation =
				new LoadSchedulesForRequestWithoutResourceCalculation(new FakePersonAbsenceAccountRepository(), _scheduleStorage);
		}

		internal void UseSpecificationCheckerWithConfig(IEnumerable<IShiftTradeSpecification> shiftTradeSpecifications,
			IGlobalSettingDataRepository globalSettingDataRepository)
		{
			var specificatonChecker = new SpecificationCheckerWithConfig(shiftTradeSpecifications, globalSettingDataRepository);
			_validator = new ShiftTradeValidator(new FakeShiftTradeLightValidator(), specificatonChecker);
		}

		internal List<IShiftTradeSpecification> GetDefaultShiftTradeSpecifications()
		{
			var shiftTradeSpecifications = new List<IShiftTradeSpecification>
			{
				new ValidatorSpecificationForTest(true, "_openShiftTradePeriodSpecification"),
				new ShiftTradeMaxSeatsSpecification(_globalSettingDataRepository, _shiftTradeMaxSeatReadModelValidator)
			};
			return shiftTradeSpecifications;
		}

		internal void UseMaxSeatReadModelValidator()
		{
			setValidator(_shiftTradeMaxSeatReadModelValidator);
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
				new ValidatorSpecificationForTest (true, "_openShiftTradePeriodSpecification"),
				new ShiftTradeMaxSeatsSpecification (_globalSettingDataRepository, shiftTradeMaxSeatValidator)
			};
			var globalSettingDataRepository = new FakeGlobalSettingDataRepository();
			globalSettingDataRepository.PersistSettingValue(ShiftTradeSettings.SettingsKey, new ShiftTradeSettings
			{
				BusinessRuleConfigs = new ShiftTradeBusinessRuleConfig[] { }
			});

			var specificationChecker = new SpecificationCheckerWithConfig(GetDefaultShiftTradeSpecifications(), globalSettingDataRepository);

			_validator = new ShiftTradeValidator(new FakeShiftTradeLightValidator(), specificationChecker);
			_activeShiftTradeMaxSeatValidator = shiftTradeMaxSeatValidator;
		}

		internal IPerson CreatePersonInTeam(ITeam team)
		{
			var workControlSet = CreateWorkFlowControlSet(true);
			var startDate = new DateOnly(2016, 1, 1);
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, team);
			((Person)person).InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
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

			SetScheduleDictionary(scheduleDictionary);

			HandleRequest(@event, businessRuleProvider);
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

			var shiftTradeSwapDetails = ((ShiftTradeRequest)personRequest.Request).ShiftTradeSwapDetails;
			foreach (var shiftTradeSwapDetail in shiftTradeSwapDetails)
			{
				shiftTradeSwapDetail.ChecksumFrom =
					new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartFrom).CalculateChecksum();
				shiftTradeSwapDetail.ChecksumTo =
					new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartTo).CalculateChecksum();
			}

			_personRequestRepository.Add(personRequest);
			return personRequest;
		}

		internal void SetScheduleDictionary(IScheduleDictionary scheduleDictionary)
		{
			_requestFactory.SetScheduleDictionary(scheduleDictionary);
			
		}

		internal void HandleRequest(AcceptShiftTradeEvent acceptShiftTradeEvent, IBusinessRuleProvider businessRuleProvider = null)
		{
			setShiftTradeRequestHandler(businessRuleProvider);
			_target.Handle(acceptShiftTradeEvent);
		}

		internal void HandleRequest(NewShiftTradeRequestCreatedEvent newShiftTradeRequestCreatedEvent, IBusinessRuleProvider businessRuleProvider = null)
		{
			setShiftTradeRequestHandler(businessRuleProvider);
			_target.Handle(newShiftTradeRequestCreatedEvent);
		}

		private void setShiftTradeRequestHandler(IBusinessRuleProvider businessRuleProvider)
		{
			var authorization = new PersonRequestAuthorizationCheckerForTest();
			var differenceService = new DifferenceEntityCollectionService<IPersistableScheduleData>();
			var shiftTradeApproveService = new ShiftTradeApproveService(authorization, differenceService,
				_scheduleDifferenceSaver, _requestFactory, _currentScenario);
			_target = new ShiftTradeRequestHandler(_schedulingResultStateHolder, _validator, _requestFactory,
				_currentScenario, _personRequestRepository, _scheduleStorage, _personRepository
				, authorization,
				_loadSchedulingDataForRequestWithoutResourceCalculation,
				businessRuleProvider ?? _businessRuleProvider,
				_shiftTradePendingReasonsService,
				shiftTradeApproveService);
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
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, dateTimePeriod, new ShiftCategory("AM")).WithId() :
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, dateTimePeriod).WithId();
			_scheduleStorage.Add(personAssignment);
			return personAssignment;
		}

		internal void SetSiteOpenHours(IPerson person, int startHour, int endHour)
		{
			var startDate = new DateOnly(2016, 1, 1);
			var team = person.MyTeam(startDate);
			var siteOpenHour = new SiteOpenHour()
			{
				IsClosed = true,
				Parent = team.Site,
				TimePeriod = new TimePeriod(startHour, 0, endHour, 0),
				WeekDay = DayOfWeek.Monday
			};
			team.Site.AddOpenHour(siteOpenHour);
		}
	}

	internal class ActivityAndDateTime
	{
		public DateTimePeriod Period { get; set; }
		public IActivity Activity { get; set; }
	}
}