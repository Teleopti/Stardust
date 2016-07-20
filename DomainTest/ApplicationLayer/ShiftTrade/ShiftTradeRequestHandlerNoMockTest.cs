using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.DomainTest.WorkflowControl.ShiftTrades;
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
		private IRequestFactory _requestFactory;
		private IShiftTradeValidator _validator;
		private IPersonRepository _personRepository;
		private IScheduleStorage _scheduleStorage;
		private IPersonAssignment _personAssignment;
		private IBusinessRuleProvider _businessRuleProvider;
		private INewBusinessRuleCollection _businessRuleCollection;
		private IScheduleDifferenceSaver _scheduleDifferenceSaver;


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

			_scheduleDifferenceSaver = new FakeScheduleDifferenceSaver(_scheduleStorage);

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
			var personRequest = prepareAndGetPersonRequest(personFrom, personTo);
			var approvalService = new ApprovalServiceForTest();
			approvalService.SetBusinessRuleResponse(ruleResponse1, ruleResponse2);

			((FakeRequestFactory)_requestFactory).setRequestApprovalService(approvalService);

			_target = new ShiftTradeRequestHandler(_schedulingResultStateHolder, _validator, _requestFactory,
				_scenarioRepository, _personRequestRepository, _scheduleStorage, _personRepository
			  , null, null,
			 _loadSchedulingDataForRequestWithoutResourceCalculation, null, null, _businessRuleProvider);

			_target.Handle(getAcceptShiftTradeEvent(personTo, personRequest.Id.Value));

			Assert.IsTrue((personRequest.BrokenBusinessRules.HasFlag(BusinessRuleFlags.MinWeeklyRestRule)));
			Assert.IsTrue((personRequest.BrokenBusinessRules.HasFlag(BusinessRuleFlags.NewMaxWeekWorkTimeRule)));

		}


		[Test]
		public void ShouldTradeShifts()
		{
			var workflowControlSet = createWorkFlowControlSet(true);

			var personTo = PersonFactory.CreatePerson("To").WithId();
			var personFrom = PersonFactory.CreatePerson("With").WithId();

			var activityPersonTo = new Activity("Shift_PersonTo").WithId();
			var activityPersonFrom = new Activity("Shift_PersonFrom").WithId();

			var dateTimePeriod = new DateTimePeriod(DateTime.Today.AddHours(8).ToUniversalTime(), DateTime.Today.AddHours(8).ToUniversalTime());
			
			addPersonAssignment(personTo, dateTimePeriod, activityPersonTo); 
			addPersonAssignment(personFrom, dateTimePeriod, activityPersonFrom);

			personTo.WorkflowControlSet = workflowControlSet;
			personFrom.WorkflowControlSet = workflowControlSet;

			prepareBusinessRuleProvider();

			_personRepository.Add(personFrom);
			_personRepository.Add(personTo);

			var personRequest = prepareAndGetPersonRequest(personFrom, personTo);

			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, null, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), _scenarioRepository.Current());
			_schedulingResultStateHolder.Schedules = scheduleDictionary;
			_schedulingResultStateHolder.Schedules.TakeSnapshot();

			var approvalService = new RequestApprovalServiceScheduler(scheduleDictionary, _scenarioRepository.Current(), 
				new SwapAndModifyService (new SwapService(), new DoNothingScheduleDayChangeCallBack()), _businessRuleCollection, 
				new DoNothingScheduleDayChangeCallBack(), new FakeGlobalSettingDataRepository() );
			
			((FakeRequestFactory)_requestFactory).setRequestApprovalService(approvalService);

			handleRequest(getAcceptShiftTradeEvent(personTo, personRequest.Id.Value));

			var personToSchedule = scheduleDictionary[personTo].ScheduledDay(DateOnly.Today);
			var personFromSchedule = scheduleDictionary[personFrom].ScheduledDay(DateOnly.Today);

			Assert.IsTrue(personRequest.IsApproved);
			Assert.IsTrue(personToSchedule.PersonAssignment().ShiftLayers.Single().Payload.Id == activityPersonFrom.Id);
			Assert.IsTrue(personFromSchedule.PersonAssignment().ShiftLayers.Single().Payload.Id == activityPersonTo.Id);

		}

		private void handleRequest (AcceptShiftTradeEvent acceptShiftTradeEvent)
		{

			_target = new ShiftTradeRequestHandler (_schedulingResultStateHolder, _validator, _requestFactory,
				_scenarioRepository, _personRequestRepository, _scheduleStorage, _personRepository
				, new PersonRequestAuthorizationCheckerForTest(), _scheduleDifferenceSaver,
				_loadSchedulingDataForRequestWithoutResourceCalculation,
				new DifferenceEntityCollectionService<IPersistableScheduleData>(), null, _businessRuleProvider);
			_target.Handle (acceptShiftTradeEvent);
		}

		private IPersonRequest prepareAndGetPersonRequest(IPerson personFrom, IPerson personTo)
		{
			var personRequest = new PersonRequestFactory().CreatePersonShiftTradeRequest(personFrom, personTo, DateOnly.Today).WithId();
			_personRequestRepository.Add(personRequest);
			return personRequest;
		}
		private void prepareBusinessRuleProvider(params IBusinessRuleResponse[] ruleResponses)
		{
			((FakeNewBusinessRuleCollection)_businessRuleCollection).SetRuleResponse(ruleResponses);
			((FakeBusinessRuleProvider)_businessRuleProvider).SetBusinessRules(_businessRuleCollection);

		}
		private IPersonAssignment addPersonAssignment(IPerson person, DateTimePeriod dateTimePeriod, IActivity activity = null)
		{
			_personAssignment = activity != null ? 
				PersonAssignmentFactory.CreateAssignmentWithMainShift (activity, person, dateTimePeriod, new ShiftCategory ("AM"), _scenarioRepository.Current()).WithId() : 
				PersonAssignmentFactory.CreateAssignmentWithMainShift (person, dateTimePeriod);
				
			_scheduleStorage.Add(_personAssignment);
			return _personAssignment;
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
				LogOnBusinessUnitId = new Guid ("928DD0BC-BF40-412E-B970-9B5E015AADEA"),
				Timestamp = DateTime.UtcNow,
				PersonRequestId = requestId, //new Guid ("9AC8476B-9B8F-4330-9561-9D7A00BAA585"),
				Message = "I want to trade!",
				AcceptingPersonId = personTo.Id.GetValueOrDefault()
			};
			return ast;
		}
	}
}
