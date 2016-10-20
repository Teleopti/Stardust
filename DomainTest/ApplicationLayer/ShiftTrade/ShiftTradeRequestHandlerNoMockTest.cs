using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[TestFixture]
	public class ShiftTradeRequestHandlerNoMockTest
	{
		private ShiftTradeRequestHandler _target;
		private IPersonRequestRepository _personRequestRepository;
		private ICurrentScenario _currentScenario;
		private ILoadSchedulesForRequestWithoutResourceCalculation _loadSchedulingDataForRequestWithoutResourceCalculation;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private FakeRequestFactory _requestFactory;
		private IPersonRepository _personRepository;
		private IScheduleStorage _scheduleStorage;
		private IBusinessRuleProvider _businessRuleProvider;
		private INewBusinessRuleCollection _businessRuleCollection;
		private IShiftTradePendingReasonsService _shiftTradePendingReasonsService;
		private ShiftTradeTestHelper _shiftTradeTestHelper;

		[SetUp]
		public void Setup()
		{
			_personRequestRepository = new FakePersonRequestRepository();
			_currentScenario = new FakeCurrentScenario();
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_personRepository = new FakePersonRepository();
			_requestFactory = new FakeRequestFactory();
			_scheduleStorage = new FakeScheduleStorage();
			_businessRuleProvider = new FakeBusinessRuleProvider();
			_businessRuleCollection = new FakeNewBusinessRuleCollection();

			_shiftTradePendingReasonsService = new ShiftTradePendingReasonsService (_requestFactory, _currentScenario);

			_loadSchedulingDataForRequestWithoutResourceCalculation =
				new LoadSchedulesForRequestWithoutResourceCalculation (new FakePersonAbsenceAccountRepository(), _scheduleStorage);

			_shiftTradeTestHelper = new ShiftTradeTestHelper (_schedulingResultStateHolder, _scheduleStorage, _personRepository,
				_businessRuleProvider, _businessRuleCollection, _currentScenario, new FakeScheduleProjectionReadOnlyActivityProvider());
		}

		[Test]
		public void ShouldGetAndHandleBrokenBusinessRules()
		{
			var personTo = PersonFactory.CreatePersonWithId();
			var personFrom = PersonFactory.CreatePersonWithId();

			var dateTimePeriod = new DateTimePeriod(DateTime.Today.AddHours(8).ToUniversalTime(), DateTime.Today.AddHours(8).ToUniversalTime());
			_shiftTradeTestHelper.AddPersonAssignment(personTo, dateTimePeriod);

			var ruleResponse1 = new BusinessRuleResponse(typeof(MinWeeklyRestRule), "no go", true, false,
				new DateTimePeriod(), personTo, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), "tjillevippen");
			var ruleResponse2 = new BusinessRuleResponse(typeof(NewMaxWeekWorkTimeRule), "no go", true, false,
				new DateTimePeriod(), personTo, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), "tjillevippen");

			prepareBusinessRuleProvider(ruleResponse1, ruleResponse2);

			_personRepository.Add(personTo);

			var personRequest = new PersonRequestFactory().CreatePersonShiftTradeRequest(personFrom, personTo, DateOnly.Today).WithId();
			new ShiftTradeSwapScheduleDetailsMapper(_scheduleStorage, _currentScenario).Map((ShiftTradeRequest)personRequest.Request);
			_personRequestRepository.Add(personRequest);

			var approvalService = new ApprovalServiceForTest();
			approvalService.SetBusinessRuleResponse(ruleResponse1, ruleResponse2);

			_requestFactory.setRequestApprovalService(approvalService);

			var shiftTradeSpecifications = new List<IShiftTradeSpecification>
			{
				new ShiftTradeValidatorTest.ValidatorSpecificationForTest(true, "_openShiftTradePeriodSpecification")

			};
			var validator = new ShiftTradeValidator(new FakeShiftTradeLightValidator(), shiftTradeSpecifications);

			_target = new ShiftTradeRequestHandler(_schedulingResultStateHolder, validator, _requestFactory,
				_currentScenario, _personRequestRepository, _scheduleStorage, _personRepository
			  , null, null,
			 _loadSchedulingDataForRequestWithoutResourceCalculation, null, _businessRuleProvider, _shiftTradePendingReasonsService);

			_target.Handle(_shiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, personRequest.Id.Value));

			Assert.IsTrue(personRequest.BrokenBusinessRules.Value.HasFlag(BusinessRuleFlags.MinWeeklyRestRule));
			Assert.IsTrue(personRequest.BrokenBusinessRules.Value.HasFlag(BusinessRuleFlags.NewMaxWeekWorkTimeRule));

		}

		[Test]
		public void ShouldTradeShiftsOnAutoApproval()
		{
			var workflowControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(true);
			var result = doBasicShiftTrade(workflowControlSet);

			Assert.IsTrue(result.PersonRequest.IsApproved);
			Assert.IsTrue(result.PersonToSchedule.PersonAssignment().ShiftLayers.Single().Payload.Id == result.ActivityFrom.Id);
			Assert.IsTrue(result.PersonFromSchedule.PersonAssignment().ShiftLayers.Single().Payload.Id == result.ActivityTo.Id);
		}

		[Test]
		public void ShouldNotTradeShiftsOGetBrokenBusinessRulesWhenToggle39473IsOff()
		{
			var workflowControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(false);

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
				_shiftTradeTestHelper.AddPersonAssignment(personTo, dateTimePeriod, activityPersonTo);
				_shiftTradeTestHelper.AddPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);
			}

			var personRequest = _shiftTradeTestHelper.PrepareAndGetPersonRequest(personFrom, personTo, scheduleDateOnly);

			_shiftTradeTestHelper.SetPersonAccounts(personTo, personFrom, scheduleDateOnly);

			var @event = _shiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, personRequest.Id.GetValueOrDefault());
			@event.UseMinWeekWorkTime = true;
			_schedulingResultStateHolder.UseMinWeekWorkTime = @event.UseMinWeekWorkTime;

			var businessRuleProvider = new BusinessRuleProvider();
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] {personTo, personFrom}, null,
				new DateOnlyPeriod(new DateOnly(scheduleDate), new DateOnly(scheduleDate.AddDays(7))), _currentScenario.Current());
			_shiftTradeTestHelper.SetApprovalService(scheduleDictionary, businessRuleProvider.GetAllBusinessRules(_schedulingResultStateHolder));

			_shiftTradeTestHelper.HandleRequest(@event, false, businessRuleProvider);

			Assert.IsTrue(personRequest.BrokenBusinessRules.Value.HasFlag(BusinessRuleFlags.MinWeekWorkTimeRule));
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
			_shiftTradeTestHelper.AddPersonAssignment(personTo, dateTimePeriod, activityPersonTo);
			_shiftTradeTestHelper.AddPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);

			var personRequest = _shiftTradeTestHelper.PrepareAndGetPersonRequest(personFrom, personTo, scheduleDateOnly);

			_shiftTradeTestHelper.SetPersonAccounts(personTo, personFrom, scheduleDateOnly);

			var @event = _shiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, personRequest.Id.GetValueOrDefault());
			@event.UseSiteOpenHoursRule = true;

			var businessRuleProvider = new BusinessRuleProvider();
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, null,
				new DateOnlyPeriod(new DateOnly(scheduleDate), new DateOnly(scheduleDate.AddDays(7))), _currentScenario.Current());
			var businessRules = businessRuleProvider.GetAllBusinessRules(_schedulingResultStateHolder);
			businessRules.Add(new SiteOpenHoursRule(new SiteOpenHoursSpecification()));
			_shiftTradeTestHelper.SetApprovalService(scheduleDictionary, businessRules);

			_shiftTradeTestHelper.HandleRequest(@event, false, businessRuleProvider);

			Assert.IsTrue(personRequest.BrokenBusinessRules.Value.HasFlag(BusinessRuleFlags.SiteOpenHoursRule));
		}


		private basicShiftTradeTestResult doBasicShiftTrade(IWorkflowControlSet workflowControlSet, bool addBrokenBusinessRules = false, bool toggle39473IsOff = false)
		{
			var personTo = PersonFactory.CreatePerson("To").WithId();
			var personFrom = PersonFactory.CreatePerson("With").WithId();

			var activityPersonTo = new Activity("Shift_PersonTo").WithId();
			var activityPersonFrom = new Activity("Shift_PersonFrom").WithId();

			var dateTimePeriod = new DateTimePeriod(DateTime.Today.AddHours(8).ToUniversalTime(), DateTime.Today.AddHours(8).ToUniversalTime());

			_shiftTradeTestHelper.AddPersonAssignment(personTo, dateTimePeriod, activityPersonTo);
			_shiftTradeTestHelper.AddPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, null, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), _currentScenario.Current());

			personTo.WorkflowControlSet = workflowControlSet;
			personFrom.WorkflowControlSet = workflowControlSet;

			if (addBrokenBusinessRules)
			{
				var ruleResponse1 = new BusinessRuleResponse(typeof(MinWeeklyRestRule), "no go", true, false, new DateTimePeriod(),
					personTo, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), "tjillevippen");
				var ruleResponse2 = new BusinessRuleResponse(typeof(NewMaxWeekWorkTimeRule), "no go", true, false,
					new DateTimePeriod(), personTo, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), "tjillevippen");
				prepareBusinessRuleProvider(ruleResponse1, ruleResponse2);
			}
			else
			{
				prepareBusinessRuleProvider();
			}


			_personRepository.Add(personTo);

			var personRequest = _shiftTradeTestHelper.PrepareAndGetPersonRequest(personFrom, personTo, DateOnly.Today);

			_shiftTradeTestHelper.SetApprovalService(scheduleDictionary);

			_shiftTradeTestHelper.HandleRequest(_shiftTradeTestHelper.GetAcceptShiftTradeEvent(personTo, personRequest.Id.Value), toggle39473IsOff);

			return new basicShiftTradeTestResult()
			{
				ActivityTo = activityPersonTo,
				ActivityFrom = activityPersonFrom,
				PersonToSchedule = scheduleDictionary[personTo].ScheduledDay(DateOnly.Today),
				PersonFromSchedule = scheduleDictionary[personFrom].ScheduledDay(DateOnly.Today),
				PersonRequest = personRequest
			};
		}
		

		private void prepareBusinessRuleProvider(params IBusinessRuleResponse[] ruleResponses)
		{
			((FakeNewBusinessRuleCollection)_businessRuleCollection).SetRuleResponse(ruleResponses);
			((FakeBusinessRuleProvider)_businessRuleProvider).SetBusinessRules(_businessRuleCollection);

		}



		private IPerson createPersonWithMinTimePerWeek(DateOnly scheduleDateOnly)
		{
			var workControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(true);
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
			var workControlSet = ShiftTradeTestHelper.CreateWorkFlowControlSet(true);
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

