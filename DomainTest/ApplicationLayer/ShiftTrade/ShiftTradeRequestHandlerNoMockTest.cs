using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
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
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[TestFixture]
	public class ShiftTradeRequestHandlerNoMockTest
	{
		private ShiftTradeRequestHandler _target;
		private IPersonRequestRepository _personRequestRepository;
		private ICurrentScenario _scenarioRepository;
		private ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulingDataForRequestWithoutResourceCalculation;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private FakeRequestFactory _requestFactory;
		private IShiftTradeValidator _validator;
		private IPersonRepository _personRepository;
		private IScheduleStorage _scheduleStorage;
		private IPersonAssignment _personAssignment;
		private IBusinessRuleProvider _businessRuleProvider;
		private INewBusinessRuleCollection _businessRuleCollection;
		private IScheduleDifferenceSaver _scheduleDifferenceSaver;
		private IShiftTradePendingReasonsService _shiftTradePendingReasonsService;

		[SetUp]
		public void Setup()
		{
			_personRequestRepository = new FakePersonRequestRepository();
			_scenarioRepository = new FakeCurrentScenario();
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_personRepository = new FakePersonRepository();
			_requestFactory = new FakeRequestFactory();
			_scheduleStorage = new FakeScheduleStorage();
			_businessRuleProvider = new FakeBusinessRuleProvider();
			_businessRuleCollection = new FakeNewBusinessRuleCollection();

			_scheduleDifferenceSaver = new ScheduleDifferenceSaver(_scheduleStorage);
			_shiftTradePendingReasonsService = new ShiftTradePendingReasonsService(_requestFactory, _scenarioRepository);

			var shiftTradeSpecifications = new List<IShiftTradeSpecification>
			{
				new ShiftTradeValidatorTest.ValidatorSpecificationForTest(true, "_openShiftTradePeriodSpecification")
			};
			_validator = new ShiftTradeValidator(new FakeShiftTradeLightValidator(), shiftTradeSpecifications);
			_loadSchedulingDataForRequestWithoutResourceCalculation = new LoadSchedulesForRequestWithoutResourceCalculation(new FakePersonAbsenceAccountRepository(), _scheduleStorage);
		}

		[Test]
		public void ShouldGetAndHandleBrokenBusinessRules()
		{
			var personTo = PersonFactory.CreatePersonWithId();
			var personFrom = PersonFactory.CreatePersonWithId();

			var dateTimePeriod = new DateTimePeriod(DateTime.Today.AddHours(8).ToUniversalTime(), DateTime.Today.AddHours(8).ToUniversalTime());
			addPersonAssignment(personTo, dateTimePeriod);

			var ruleResponse1 = new BusinessRuleResponse(typeof(MinWeeklyRestRule), "no go", true, false,
				new DateTimePeriod(), personTo, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today));
			var ruleResponse2 = new BusinessRuleResponse(typeof(NewMaxWeekWorkTimeRule), "no go", true, false,
				new DateTimePeriod(), personTo, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today));

			prepareBusinessRuleProvider(ruleResponse1, ruleResponse2);

			_personRepository.Add(personTo);
			var personRequest = prepareAndGetPersonRequest(personFrom, personTo, DateOnly.Today);
			var approvalService = new ApprovalServiceForTest();
			approvalService.SetBusinessRuleResponse(ruleResponse1, ruleResponse2);

			_requestFactory.setRequestApprovalService(approvalService);

			_target = new ShiftTradeRequestHandler(_schedulingResultStateHolder, _validator, _requestFactory,
				_scenarioRepository, _personRequestRepository, _scheduleStorage, _personRepository
			  , null, null,
			 _loadSchedulingDataForRequestWithoutResourceCalculation, null, _businessRuleProvider, _shiftTradePendingReasonsService);

			_target.Handle(getAcceptShiftTradeEvent(personTo, personRequest.Id.Value));

			Assert.IsTrue(personRequest.BrokenBusinessRules.HasFlag(BusinessRuleFlags.MinWeeklyRestRule));
			Assert.IsTrue(personRequest.BrokenBusinessRules.HasFlag(BusinessRuleFlags.NewMaxWeekWorkTimeRule));

		}

		[Test]
		public void ShouldTradeShiftsOnAutoApproval()
		{
			var workflowControlSet = createWorkFlowControlSet(true);
			var result = doBasicShiftTrade(workflowControlSet);

			Assert.IsTrue(result.PersonRequest.IsApproved);
			Assert.IsTrue(result.PersonToSchedule.PersonAssignment().ShiftLayers.Single().Payload.Id == result.ActivityFrom.Id);
			Assert.IsTrue(result.PersonFromSchedule.PersonAssignment().ShiftLayers.Single().Payload.Id == result.ActivityTo.Id);
		}

		[Test]
		public void ShouldNotTradeShiftsOGetBrokenBusinessRulesWhenToggle39473IsOff()
		{
			var workflowControlSet = createWorkFlowControlSet(false);

			var result = doBasicShiftTrade(workflowControlSet, true, true);

			Assert.IsFalse(result.PersonRequest.IsApproved);
			Assert.IsTrue(result.PersonToSchedule.PersonAssignment().ShiftLayers.Single().Payload.Id == result.ActivityTo.Id);
			Assert.IsTrue(result.PersonFromSchedule.PersonAssignment().ShiftLayers.Single().Payload.Id == result.ActivityFrom.Id);
		}

		[Test]
		public void ShouldSetMinWeeklyWorkTimeBrokenRuleWhenUseMinWeekWorkTimeIsOn()
		{
			var scheduleDate = new DateTime(2016, 7, 25);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var personTo = createPersonWithMinTimePerWeek(scheduleDateOnly);
			var activityPersonTo = new Activity("Shift_PersonTo").WithId();

			var personFrom = createPersonWithMinTimePerWeek(scheduleDateOnly);
			var activityPersonFrom = new Activity("Shift_PersonFrom").WithId();

			for (var i = 0; i < 7; i++)
			{
				var dateTimePeriod = new DateTimePeriod(scheduleDate.AddDays(i).AddHours(8).ToUniversalTime(),
				scheduleDate.AddDays(i).AddHours(10).ToUniversalTime());
				addPersonAssignment(personTo, dateTimePeriod, activityPersonTo);
				addPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);
			}

			var personRequest = prepareAndGetPersonRequest(personFrom, personTo, scheduleDateOnly);

			setPersonAccounts(personTo, personFrom, scheduleDateOnly);

			var @event = getAcceptShiftTradeEvent(personTo, personRequest.Id.GetValueOrDefault());
			@event.UseMinWeekWorkTime = true;
			_schedulingResultStateHolder.UseMinWeekWorkTime = @event.UseMinWeekWorkTime;

			var businessRuleProvider = new BusinessRuleProvider();
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] {personTo, personFrom}, null,
				new DateOnlyPeriod(new DateOnly(scheduleDate), new DateOnly(scheduleDate.AddDays(7))), _scenarioRepository.Current());
			setApprovalService(scheduleDictionary, businessRuleProvider.GetAllBusinessRules(_schedulingResultStateHolder));

			handleRequest(@event, false, businessRuleProvider);

			Assert.IsTrue(personRequest.BrokenBusinessRules.HasFlag(BusinessRuleFlags.MinWeekWorkTimeRule));
		}

		[Test]
		public void ShouldSetSiteOpenHoursBrokenRule()
		{
			var scheduleDate = new DateTime(2016, 7, 25);
			var scheduleDateOnly = new DateOnly(scheduleDate);

			var personTo = createPersonWithSiteOpenHours(8, 17);
			var activityPersonTo = new Activity("Shift_PersonTo").WithId();

			var personFrom = createPersonWithSiteOpenHours(8, 17);
			var activityPersonFrom = new Activity("Shift_PersonFrom").WithId();

			var dateTimePeriod = scheduleDateOnly.ToDateTimePeriod(new TimePeriod(8, 0, 18, 0),
				personTo.PermissionInformation.DefaultTimeZone());
			addPersonAssignment(personTo, dateTimePeriod, activityPersonTo);
			addPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);

			var personRequest = prepareAndGetPersonRequest(personFrom, personTo, scheduleDateOnly);

			setPersonAccounts(personTo, personFrom, scheduleDateOnly);

			var @event = getAcceptShiftTradeEvent(personTo, personRequest.Id.GetValueOrDefault());
			@event.UseSiteOpenHoursRule = true;

			var businessRuleProvider = new BusinessRuleProvider();
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, null,
				new DateOnlyPeriod(new DateOnly(scheduleDate), new DateOnly(scheduleDate.AddDays(7))), _scenarioRepository.Current());
			var businessRules = businessRuleProvider.GetAllBusinessRules(_schedulingResultStateHolder);
			businessRules.Add(new SiteOpenHoursRule());
			setApprovalService(scheduleDictionary, businessRules);

			handleRequest(@event, false, businessRuleProvider);

			Assert.IsTrue(personRequest.BrokenBusinessRules.HasFlag(BusinessRuleFlags.SiteOpenHoursRule));
		}

		private basicShiftTradeTestResult doBasicShiftTrade(IWorkflowControlSet workflowControlSet, bool addBrokenBusinessRules = false, bool toggle39473IsOff = false)
		{
			var personTo = PersonFactory.CreatePerson("To").WithId();
			var personFrom = PersonFactory.CreatePerson("With").WithId();

			var activityPersonTo = new Activity("Shift_PersonTo").WithId();
			var activityPersonFrom = new Activity("Shift_PersonFrom").WithId();

			var dateTimePeriod = new DateTimePeriod(DateTime.Today.AddHours(8).ToUniversalTime(), DateTime.Today.AddHours(8).ToUniversalTime());

			addPersonAssignment(personTo, dateTimePeriod, activityPersonTo);
			addPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, null, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), _scenarioRepository.Current());

			personTo.WorkflowControlSet = workflowControlSet;
			personFrom.WorkflowControlSet = workflowControlSet;

			if (addBrokenBusinessRules)
			{
				var ruleResponse1 = new BusinessRuleResponse(typeof(MinWeeklyRestRule), "no go", true, false, new DateTimePeriod(),
					personTo, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today));
				var ruleResponse2 = new BusinessRuleResponse(typeof(NewMaxWeekWorkTimeRule), "no go", true, false,
					new DateTimePeriod(), personTo, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today));
				prepareBusinessRuleProvider(ruleResponse1, ruleResponse2);
			}
			else
			{
				prepareBusinessRuleProvider();
			}


			_personRepository.Add(personTo);

			var personRequest = prepareAndGetPersonRequest(personFrom, personTo, DateOnly.Today);

			setApprovalService(scheduleDictionary);

			handleRequest(getAcceptShiftTradeEvent(personTo, personRequest.Id.Value), toggle39473IsOff);

			return new basicShiftTradeTestResult()
			{
				ActivityTo = activityPersonTo,
				ActivityFrom = activityPersonFrom,
				PersonToSchedule = scheduleDictionary[personTo].ScheduledDay(DateOnly.Today),
				PersonFromSchedule = scheduleDictionary[personFrom].ScheduledDay(DateOnly.Today),
				PersonRequest = personRequest
			};
		}

		private void setApprovalService(IScheduleDictionary scheduleDictionary, INewBusinessRuleCollection newBusinessRules = null)
		{
			var approvalService = new RequestApprovalServiceScheduler(scheduleDictionary, _scenarioRepository.Current(),
				new SwapAndModifyService(new SwapService(), new DoNothingScheduleDayChangeCallBack()), newBusinessRules ?? _businessRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new FakeGlobalSettingDataRepository());

			_requestFactory.setRequestApprovalService(approvalService);
		}

		private void handleRequest(AcceptShiftTradeEvent acceptShiftTradeEvent, bool toggle39473IsOff = false, IBusinessRuleProvider businessRuleProvider = null)
		{

			_target = new ShiftTradeRequestHandler(_schedulingResultStateHolder, _validator, _requestFactory,
				_scenarioRepository, _personRequestRepository, _scheduleStorage, _personRepository
				, new PersonRequestAuthorizationCheckerForTest(), _scheduleDifferenceSaver,
				_loadSchedulingDataForRequestWithoutResourceCalculation,
				new DifferenceEntityCollectionService<IPersistableScheduleData>(),
				businessRuleProvider ?? _businessRuleProvider,
				toggle39473IsOff ? new ShiftTradePendingReasonsService39473ToggleOff() : _shiftTradePendingReasonsService);
			_target.Handle(acceptShiftTradeEvent);
		}

		private IPersonRequest prepareAndGetPersonRequest(IPerson personFrom, IPerson personTo, DateOnly shiftTradeDate)
		{
			var personRequest = new PersonRequestFactory().CreatePersonShiftTradeRequest(personFrom, personTo, shiftTradeDate).WithId();
			_personRequestRepository.Add(personRequest);
			return personRequest;
		}

		private void prepareBusinessRuleProvider(params IBusinessRuleResponse[] ruleResponses)
		{
			((FakeNewBusinessRuleCollection)_businessRuleCollection).SetRuleResponse(ruleResponses);
			((FakeBusinessRuleProvider)_businessRuleProvider).SetBusinessRules(_businessRuleCollection);

		}

		private void addPersonAssignment(IPerson person, DateTimePeriod dateTimePeriod, IActivity activity = null)
		{
			var scenario = _scenarioRepository.Current();
			_personAssignment = activity != null ?
				PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, dateTimePeriod, new ShiftCategory("AM"), scenario).WithId() :
				PersonAssignmentFactory.CreateAssignmentWithMainShift(scenario, person, dateTimePeriod);
			_scheduleStorage.Add(_personAssignment);
		}

		private static WorkflowControlSet createWorkFlowControlSet(bool autoGrantShiftTrade)
		{
			var workflowControlSet = new WorkflowControlSet { AutoGrantShiftTradeRequest = autoGrantShiftTrade }.WithId();
			return workflowControlSet;

		}

		private AcceptShiftTradeEvent getAcceptShiftTradeEvent(IPerson personTo, Guid requestId)
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

		private IPerson createPersonWithMinTimePerWeek(DateOnly scheduleDateOnly)
		{
			var workControlSet = createWorkFlowControlSet(true);
			var minTimePerWeek = TimeSpan.FromHours(40);
			var startDate = new DateOnly(2016, 1, 1);
			var team = TeamFactory.CreateSimpleTeam();

			var person = PersonFactory.CreatePerson("Person").WithId();
			person.WorkflowControlSet = workControlSet;
			var personToContract = PersonContractFactory.CreatePersonContract();
			person.AddPersonPeriod(new PersonPeriod(startDate, personToContract, team));
			person.Period(scheduleDateOnly).PersonContract.Contract.WorkTimeDirective = new WorkTimeDirective(minTimePerWeek,
				TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);
			_personRepository.Add(person);

			return person;
		}

		private IPerson createPersonWithSiteOpenHours(int startHour, int endHour)
		{
			var workControlSet = createWorkFlowControlSet(true);
			var startDate = new DateOnly(2016, 1, 1);
			var team = TeamFactory.CreateTeam("team", "site");
			var siteOpenHour = new SiteOpenHour()
			{
				IsClosed = true,
				Parent = team.Site,
				TimePeriod = new TimePeriod(startHour, 0, endHour, 0),
				WeekDay = DayOfWeek.Monday
			};
			team.Site.AddOpenHour(siteOpenHour);

			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(startDate, team);
			person.WorkflowControlSet = workControlSet;
			_personRepository.Add(person);

			return person;
		}

		private void setPersonAccounts(IPerson personTo, IPerson personFrom, DateOnly scheduleDateOnly)
		{
			_schedulingResultStateHolder.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>
			{
				{personTo, new PersonAccountCollection(personTo) {createPersonAbsenceAccount(personTo, scheduleDateOnly)}},
				{personFrom, new PersonAccountCollection(personFrom) {createPersonAbsenceAccount(personFrom, scheduleDateOnly)}}
			};
		}

		private PersonAbsenceAccount createPersonAbsenceAccount(IPerson person, DateOnly scheduleDateOnly)
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

		private class basicShiftTradeTestResult
		{
			public IScheduleDay PersonFromSchedule { get; set; }
			public IScheduleDay PersonToSchedule { get; set; }
			public IPersonRequest PersonRequest { get; set; }
			public IActivity ActivityTo { get; set; }
			public IActivity ActivityFrom { get; set; }
		}
	}
}

