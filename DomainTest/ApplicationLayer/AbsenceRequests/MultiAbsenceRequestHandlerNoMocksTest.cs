//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using Teleopti.Ccc.Domain.AgentInfo.Requests;
//using Teleopti.Ccc.Domain.ApplicationLayer;
//using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
//using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
//using Teleopti.Ccc.Domain.ApplicationLayer.Events;
//using Teleopti.Ccc.Domain.Budgeting;
//using Teleopti.Ccc.Domain.Common;
//using Teleopti.Ccc.Domain.FeatureFlags;
//using Teleopti.Ccc.Domain.Forecasting;
//using Teleopti.Ccc.Domain.Repositories;
//using Teleopti.Ccc.Domain.ResourceCalculation;
//using Teleopti.Ccc.Domain.Scheduling;
//using Teleopti.Ccc.Domain.WorkflowControl;
//using Teleopti.Ccc.Infrastructure.Absence;
//using Teleopti.Ccc.Infrastructure.Repositories;
//using Teleopti.Ccc.IocCommon.Toggle;
//using Teleopti.Ccc.TestCommon;
//using Teleopti.Ccc.TestCommon.FakeData;
//using Teleopti.Ccc.TestCommon.FakeRepositories;
//using Teleopti.Ccc.TestCommon.Services;
//using Teleopti.Interfaces.Domain;

//namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
//{
//	[TestFixture]
//	public class MultiAbsenceRequestHandlerNoMocksTest
//	{
//		readonly ICurrentScenario _currentScenario = new FakeCurrentScenario();
//		private IPersonRepository _personRepository;
//		private IPersonRequestRepository _personRequestRepository;
//		private SchedulingResultStateHolder _schedulingResultStateHolder;
//		readonly FakeCurrentUnitOfWorkFactory _currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();

//		private FakeScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
//		private LoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;
//		private LoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;

//		FakeScheduleDataReadScheduleStorage _scheduleRepository;
//		FakePersonAbsenceAccountRepository _personAbsenceAccountRepository;

//		private FakeScenarioRepository _scenarioRepository;
//		private FakeBudgetDayRepository _fakeBudgetDayRepository;
//		private PersonAccountUpdaterDummy _personAccountUpdaterDummy;

//		//private ApproveRequestCommandHandler _approveRequestCommandHandler;
//		//private DenyRequestCommandHandler _denyRequestCommandHandler;

//		[SetUp]
//		public void Setup()
//		{
//			_personRepository = new FakePersonRepository();
//			_personRequestRepository = new FakePersonRequestRepository();
//			_schedulingResultStateHolder = new SchedulingResultStateHolder();
//			_scheduleRepository = new FakeScheduleDataReadScheduleStorage();
//			_personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
//			_fakeBudgetDayRepository = new FakeBudgetDayRepository();
//			_scheduleProjectionReadOnlyPersister = new FakeScheduleProjectionReadOnlyPersister();

//			_personAccountUpdaterDummy = new PersonAccountUpdaterDummy();

//			var skillRepository = new FakeSkillRepository();
//			var workloadRepository = new FakeWorkloadRepository();
//			var peopleAndSkillLoaderDecider = new PeopleAndSkillLoaderDecider(_personRepository, null);
//			var skillDayLoadHelper = new SkillDayLoadHelper(new FakeSkillDayRepository(),
//				new MultisiteDayRepository(new FakeUnitOfWork()));
//			_scenarioRepository = new FakeScenarioRepository(_currentScenario.Current());
			
//			_loadSchedulesForRequestWithoutResourceCalculation = new LoadSchedulesForRequestWithoutResourceCalculation(_personAbsenceAccountRepository, _scheduleRepository);
//			_loadSchedulingStateHolderForResourceCalculation = new LoadSchedulingStateHolderForResourceCalculation(_personRepository, _personAbsenceAccountRepository, skillRepository,
//				workloadRepository, _scheduleRepository, peopleAndSkillLoaderDecider, skillDayLoadHelper);
			

//		}

//		[Test]
//		public void VerifyAbsenceRequestCanBeSetToPending()
//		{
//			var request = simpleRequestStatusTest(new PendingAbsenceRequest());
//			Assert.IsTrue(request.IsPending);
//		}

//		[Test]
//		public void VerifyAbsenceRequestCanBeSetToGranted()
//		{
//			var request = simpleRequestStatusTest(new GrantAbsenceRequest());
//			Assert.IsTrue(request.IsApproved);
//		}

//		[Test]
//		public void VerifyAbsenceRequestCanBeSetToDenied()
//		{
//			var request = simpleRequestStatusTest(new DenyAbsenceRequest());
//			Assert.IsTrue(request.IsAutoDenied);
//		}

//		[Test]
//		public void ShouldUpdateExistingPersonalAccountDataWhenConsumingAbsenceRequest()
//		{
//			simpleRequestStatusTest(new GrantAbsenceRequest(), true);
//			Assert.AreEqual(1, _personAccountUpdaterDummy.CallCount);
//		}

//		[Test]
//		public void ShouldNotUpdateExistingPersonalAccountDataWhenConsumingAbsenceRequest()
//		{
//			simpleRequestStatusTest(new GrantAbsenceRequest());
//			Assert.AreEqual(0, _personAccountUpdaterDummy.CallCount);
//		}
		

//		[Test]
//		public void ShouldNotProcessManuallyDeniedRequestForDifferentPerson()
//		{
//			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
//			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
//			var requestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
//			var absence = AbsenceFactory.CreateAbsence("Holiday");

//			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new GrantAbsenceRequest(), true);
//			var personOne = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);
//			var personTwo = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

//			var existingDeniedRequest = createAbsenceRequest(personOne, absence, requestDateTimePeriod);
//			existingDeniedRequest.Pending();
//			existingDeniedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());  // waitlist
//			existingDeniedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());  // deny

//			var newRequest = createAbsenceRequest(personTwo, absence, requestDateTimePeriod);
//			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(true, false);

//			newAbsenceRequestConsumer.Handle(new NewMultiAbsenceRequestsCreatedEvent() { PersonRequestIds = new List<Guid>() { newRequest.Id.GetValueOrDefault() } });

//			Assert.IsTrue(existingDeniedRequest.IsDenied);
//			Assert.IsFalse(existingDeniedRequest.IsAutoDenied);
//			Assert.IsFalse(existingDeniedRequest.IsWaitlisted);
//			Assert.IsTrue(newRequest.IsApproved);
//		}

//		[Test]
//		public void WhenWaitlistingDisabledShouldNotProcessPreviouslyDeniedRequest()
//		{
//			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
//			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
//			var requestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
//			var absence = AbsenceFactory.CreateAbsence("Holiday");

//			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new GrantAbsenceRequest(), false);
//			var person = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

//			var existingDeniedRequest = createAbsenceRequest(person, absence, requestDateTimePeriod);
//			existingDeniedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());

//			var newRequest = createAbsenceRequest(person, absence, requestDateTimePeriod);
//			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(false, false);
//			newAbsenceRequestConsumer.Handle(new NewMultiAbsenceRequestsCreatedEvent() { PersonRequestIds = new List<Guid>() { newRequest.Id.GetValueOrDefault() } });

//			Assert.IsTrue(newRequest.IsApproved);
//			Assert.IsTrue(existingDeniedRequest.IsAutoDenied);
//		}


//		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
//		{
//			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, requestDateTimePeriod));

//			personRequest.SetId(Guid.NewGuid());
//			_personRequestRepository.Add(personRequest);

//			return personRequest;
//		}

//		private PersonRequest simpleRequestStatusTest(IProcessAbsenceRequest processAbsenceRequest, bool forcePersonalAccountUpdate = false, bool enableWaitlisting = false)
//		{
//			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
//			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);

//			var absence = AbsenceFactory.CreateAbsence("Holiday");
//			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, processAbsenceRequest, enableWaitlisting);
//			var person = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

//			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(false, forcePersonalAccountUpdate);
//			var request = createAbsenceRequest(person, absence, new DateTimePeriod(startDateTime, endDateTime));

//			newAbsenceRequestConsumer.Handle(new NewMultiAbsenceRequestsCreatedEvent() { PersonRequestIds = new List<Guid>() { request.Id.GetValueOrDefault() } });

//			return request;
//		}

//		private IPerson createAndSetupPerson(DateTime startDateTime, DateTime endDateTime, IWorkflowControlSet workflowControlSet)
//		{
//			var person = PersonFactory.CreatePersonWithId();
//			_personRepository.Add(person);

//			var assignmentOne = createAssignment(person, startDateTime, endDateTime, _currentScenario);
//			_scheduleRepository.Set(new IScheduleData[] { assignmentOne });

//			person.WorkflowControlSet = workflowControlSet;

//			return person;
//		}

//		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence, IProcessAbsenceRequest processAbsenceRequest, bool waitlistingIsEnabled)
//		{
//			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = waitlistingIsEnabled };
//			workflowControlSet.SetId(Guid.NewGuid());

//			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

//			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
//			{
//				Absence = absence,
//				AbsenceRequestProcess = processAbsenceRequest,
//				PersonAccountValidator = new PersonAccountBalanceValidator(),
//				Period = dateOnlyPeriod,
//				OpenForRequestsPeriod = dateOnlyPeriod
//			};

//			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

//			return workflowControlSet;

//		}

//		private MultiAbsenceRequestsHandler createNewAbsenceRequestHandler(bool enableWaitlisting, bool forceAccountRecalcBeforeProcessingRequest)
//		{
//			var resourceCalculator = new ResourceCalculationPrerequisitesLoader(_currentUnitOfWorkFactory,
//				new FakeContractScheduleRepository(),
//				new FakeActivityRepository(), new FakeAbsenceRepository());

//			var requestFactory =
//				new RequestFactory(new SwapAndModifyService(new SwapService(), new DoNothingScheduleDayChangeCallBack()),
//					new PersonRequestAuthorizationCheckerForTest(), new FakeGlobalSettingDataRepository(), null);


//			var toggleManager = enableWaitlisting
//							? new FakeToggleManager(Toggles.Wfm_Requests_Waitlist_36232)
//							: new FakeToggleManager();

//			if (forceAccountRecalcBeforeProcessingRequest)
//			{
//				toggleManager.Enable(Toggles.Request_RecalculatePersonAccountBalanceOnRequestConsumer_36850);
//			}

//			var multiAbsenceRequestStatusUpdater = new MultiAbsenceRequestsUpdater(new PersonAbsenceAccountProvider(_personAbsenceAccountRepository),
//				resourceCalculator,
//				new DefaultScenarioFromRepository(_scenarioRepository),
//				_loadSchedulingStateHolderForResourceCalculation,
//				_loadSchedulesForRequestWithoutResourceCalculation,
//				requestFactory,
//				new AlreadyAbsentSpecification(),
//				new ScheduleIsInvalidSpecification(),
//				new PersonRequestCheckAuthorization(),
//				new BudgetGroupHeadCountSpecification(_scenarioRepository, _fakeBudgetDayRepository,
//					_scheduleProjectionReadOnlyPersister),
//				null,
//				new BudgetGroupAllowanceSpecification(_currentScenario, _fakeBudgetDayRepository,
//					_scheduleProjectionReadOnlyPersister),
//				new FakeScheduleDifferenceSaver(_scheduleRepository),
//				_personAccountUpdaterDummy, toggleManager);

//			var absenceProcessor = new MultiAbsenceRequestProcessor(multiAbsenceRequestStatusUpdater, () => _schedulingResultStateHolder);

//			var newAbsenceRequestConsumer = new MultiAbsenceRequestsHandler(_personRequestRepository, absenceProcessor,
//				_currentUnitOfWorkFactory);

//			return newAbsenceRequestConsumer;
//		}

//		private IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, ICurrentScenario currentScenario)
//		{
//			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
//				currentScenario.Current(),
//				person,
//				new DateTimePeriod(startDate, endDate));
//		}

//	}
//}