﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.AbsenceRequest;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Requests;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	[TestFixture]
	public class NewAbsenceRequestConsumerNoMocksTest
	{
		readonly ICurrentScenario _currentScenario = new FakeCurrentScenario();
		private IPersonRepository _personRepository;
		private IPersonRequestRepository _personRequestRepository;
		private SchedulingResultStateHolder _schedulingResultStateHolder;
		readonly FakeCurrentUnitOfWorkFactory _unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();

		private FakeScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
		private LoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;
		private LoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;
		private UpdateScheduleProjectionReadModel _scheduleProjectionReadModel;

		FakeScheduleDataReadScheduleStorage _scheduleRepository;
		FakePersonAbsenceAccountRepository _personAbsenceAccountRepository;

		private FakeScenarioRepository _scenarioRepository;
		private FakeBudgetDayRepository _fakeBudgetDayRepository;
		private PersonAccountUpdaterDummy _personAccountUpdaterDummy;

		[SetUp]
		public void Setup()
		{
			_personRepository = new FakePersonRepository();
			_personRequestRepository = new FakePersonRequestRepository();
			_schedulingResultStateHolder = new SchedulingResultStateHolder();
			_scheduleRepository = new FakeScheduleDataReadScheduleStorage();
			_personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();
			_fakeBudgetDayRepository = new FakeBudgetDayRepository();
			_scheduleProjectionReadOnlyRepository = new FakeScheduleProjectionReadOnlyRepository();

			_scheduleProjectionReadModel = new UpdateScheduleProjectionReadModel(new ProjectionChangedEventBuilder(), _scheduleProjectionReadOnlyRepository);

			_personAccountUpdaterDummy = new PersonAccountUpdaterDummy();

			var skillRepository = new FakeSkillRepository();
			var workloadRepository = new FakeWorkloadRepository();
			var peopleAndSkillLoaderDecider = new PeopleAndSkillLoaderDecider(_personRepository, null);
			var skillDayLoadHelper = new SkillDayLoadHelper(new FakeSkillDayRepository(),
				new MultisiteDayRepository(new FakeUnitOfWork()));
			_scenarioRepository = new FakeScenarioRepository(_currentScenario.Current());

			_loadSchedulesForRequestWithoutResourceCalculation = new LoadSchedulesForRequestWithoutResourceCalculation(_schedulingResultStateHolder, _personAbsenceAccountRepository, _scheduleRepository);
			_loadSchedulingStateHolderForResourceCalculation = new LoadSchedulingStateHolderForResourceCalculation(_personRepository, _personAbsenceAccountRepository, skillRepository,
				workloadRepository, _scheduleRepository, _schedulingResultStateHolder, peopleAndSkillLoaderDecider, skillDayLoadHelper);
		}

		[Test]
		public void VerifyAbsenceRequestCanBeSetToPending()
		{
			var request = simpleRequestStatusTest(new PendingAbsenceRequest());
			Assert.IsTrue(request.IsPending);
		}

		[Test]
		public void VerifyAbsenceRequestCanBeSetToGranted()
		{
			var request = simpleRequestStatusTest(new GrantAbsenceRequest());
			Assert.IsTrue(request.IsApproved);
		}

		[Test]
		public void VerifyAbsenceRequestCanBeSetToDenied()
		{
			var request = simpleRequestStatusTest(new DenyAbsenceRequest());
			Assert.IsTrue(request.IsAutoDenied);
		}
		
		[Test]
		public void ShouldUpdateExistingPersonalAccountDataWhenConsumingAbsenceRequest()
		{
			simpleRequestStatusTest(new GrantAbsenceRequest(), true);
			Assert.AreEqual(1, _personAccountUpdaterDummy.CallCount);
		}

		[Test]
		public void ShouldNotUpdateExistingPersonalAccountDataWhenConsumingAbsenceRequest()
		{
			simpleRequestStatusTest(new GrantAbsenceRequest(), false);
			Assert.AreEqual(0, _personAccountUpdaterDummy.CallCount);
		}

		[Test]
		public void ShouldProcessWaitlistedRequestForSamePerson()
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
			var requestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);

			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new GrantAbsenceRequest(), true);
			var person = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

			var existingDeniedRequest = createAbsenceRequest(person, absence, requestDateTimePeriod);
			existingDeniedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());

			var newRequest = createAbsenceRequest(person, absence, requestDateTimePeriod);
			var newAbsenceRequestConsumer = createNewAbsenceRequestConsumer(true, false);
			newAbsenceRequestConsumer.Consume(new NewAbsenceRequestCreated() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(existingDeniedRequest.IsApproved);
			Assert.IsNullOrEmpty (existingDeniedRequest.DenyReason);
			//new request should be denied as is a request for the same day as the accepted absence request
			Assert.IsTrue(newRequest.IsWaitlisted);
		}

		[Test]
		public void ShouldProcessWaitlistedRequestForDifferentPerson()
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
			var requestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new GrantAbsenceRequest(), true);

			var personOne = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);
			var personTwo = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

			var existingDeniedRequest = createAbsenceRequest(personOne, absence, requestDateTimePeriod);
			existingDeniedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());

			var newRequest = createAbsenceRequest(personTwo, absence, requestDateTimePeriod);
			var newAbsenceRequestConsumer = createNewAbsenceRequestConsumer(true, false);

			newAbsenceRequestConsumer.Consume(new NewAbsenceRequestCreated() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(existingDeniedRequest.IsApproved);
			Assert.IsTrue(newRequest.IsApproved);
		}

		[Test, Ignore("Just looking at performance")]
		public void ShouldProcessWaitlistedRequestForDifferentPeople()
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
			var requestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new GrantAbsenceRequest(), true);

			var personRequests = new PersonRequest[1000];

			for (var i = 0; i < 1000; i++)
			{
				var person = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);
				var existingDeniedRequest = createAbsenceRequest(person, absence, requestDateTimePeriod);
				existingDeniedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());
				personRequests[i] = existingDeniedRequest;
			}
			
			var personOne = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

			var newRequest = createAbsenceRequest(personOne, absence, requestDateTimePeriod);
			var newAbsenceRequestConsumer = createNewAbsenceRequestConsumer(true, false);

			var stopwatch = Stopwatch.StartNew();
			
			newAbsenceRequestConsumer.Consume(new NewAbsenceRequestCreated() { PersonRequestId = newRequest.Id.Value });
			
			stopwatch.Stop();
			Console.WriteLine(stopwatch.Elapsed);

			foreach (var  request in personRequests)
			{
				Assert.IsTrue(request.IsApproved);	
			}
			
			Assert.IsTrue(newRequest.IsApproved);
			
		}
		
		[Test]
		public void ShouldNotProcessManuallyDeniedRequestForDifferentPerson()
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
			var requestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new GrantAbsenceRequest(), true);
			var personOne = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);
			var personTwo = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

			var existingDeniedRequest = createAbsenceRequest(personOne, absence, requestDateTimePeriod);
			existingDeniedRequest.Pending(); // auto deny is always from new...lets set this to pending, so is manual deny
			existingDeniedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());

			var newRequest = createAbsenceRequest(personTwo, absence, requestDateTimePeriod);
			var newAbsenceRequestConsumer = createNewAbsenceRequestConsumer(true, false);

			newAbsenceRequestConsumer.Consume(new NewAbsenceRequestCreated() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(existingDeniedRequest.IsDenied);
			Assert.IsFalse(existingDeniedRequest.IsAutoDenied);
			Assert.IsTrue(newRequest.IsApproved);
		}

		[Test]
		public void WhenWaitlistingDisabledShouldNotProcessPreviouslyDeniedRequest()
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
			var requestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new GrantAbsenceRequest(), false);
			var person = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

			var existingDeniedRequest = createAbsenceRequest(person, absence, requestDateTimePeriod);
			existingDeniedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());

			var newRequest = createAbsenceRequest(person, absence, requestDateTimePeriod);
			var newAbsenceRequestConsumer = createNewAbsenceRequestConsumer(false, false);
			newAbsenceRequestConsumer.Consume(new NewAbsenceRequestCreated() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(newRequest.IsApproved);
			Assert.IsTrue(existingDeniedRequest.IsAutoDenied);
		}


		[Test]
		public void WhenWaitlistingShouldNotProcessPreviouslyDeniedRequestOutsideOfNewRequestPeriod()
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);

			var newRequestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			var existingWaitlistedRequestDateTimePeriod = new DateTimePeriod(startDateTime.AddDays(-1), startDateTime.AddMinutes(-1));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new GrantAbsenceRequest(), true);
			var person = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

			var existingDeniedRequest = createAbsenceRequest(person, absence, existingWaitlistedRequestDateTimePeriod);
			existingDeniedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			var newRequest = createAbsenceRequest(person, absence, newRequestDateTimePeriod);
			var newAbsenceRequestConsumer = createNewAbsenceRequestConsumer(true, false);

			newAbsenceRequestConsumer.Consume(new NewAbsenceRequestCreated() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(newRequest.IsApproved);
			Assert.IsTrue(existingDeniedRequest.IsWaitlisted);
		}

		[Test]
		public void ShouldNotUpdateWaitlistedRequestsFromDifferentWorkflowControlSets()
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
			var requestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var workflowControlSetOne = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new GrantAbsenceRequest(), true);
			var personOne = createAndSetupPerson(startDateTime, endDateTime, workflowControlSetOne);
			var workflowControlSetTwo = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new GrantAbsenceRequest(), true);
			var personTwo = createAndSetupPerson(startDateTime, endDateTime, workflowControlSetTwo);

			var existingWaitlistedRequest = createAbsenceRequest(personOne, absence, requestDateTimePeriod);
			existingWaitlistedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());

			var newRequest = createAbsenceRequest(personTwo, absence, requestDateTimePeriod);
			var newAbsenceRequestConsumer = createNewAbsenceRequestConsumer(true, false);

			newAbsenceRequestConsumer.Consume(new NewAbsenceRequestCreated() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(existingWaitlistedRequest.IsWaitlisted);
			Assert.IsTrue(newRequest.IsApproved);
		}

		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, requestDateTimePeriod));

			personRequest.SetId(Guid.NewGuid());
			_personRequestRepository.Add(personRequest);

			return personRequest;
		}

		private PersonRequest simpleRequestStatusTest(IProcessAbsenceRequest processAbsenceRequest, bool forcePersonalAccountUpdate = false, bool enableWaitlisting = false)
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, processAbsenceRequest, enableWaitlisting);
			var person = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

			var newAbsenceRequestConsumer = createNewAbsenceRequestConsumer(false, forcePersonalAccountUpdate);
			var request = createAbsenceRequest(person, absence, new DateTimePeriod(startDateTime, endDateTime));

			newAbsenceRequestConsumer.Consume(new NewAbsenceRequestCreated() { PersonRequestId = request.Id.Value });

			return request;
		}

		private IPerson createAndSetupPerson(DateTime startDateTime, DateTime endDateTime, IWorkflowControlSet workflowControlSet)
		{
			var person = PersonFactory.CreatePersonWithId();
			_personRepository.Add(person);

			var assignmentOne = createAssignment(person, startDateTime, endDateTime, _currentScenario);
			_scheduleRepository.Set(new IScheduleData[] { assignmentOne });

			person.WorkflowControlSet = workflowControlSet;

			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence, IProcessAbsenceRequest processAbsenceRequest, bool waitlistingIsEnabled)
		{
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = waitlistingIsEnabled };
			workflowControlSet.SetId (Guid.NewGuid());

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = processAbsenceRequest,
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;

		}

		private NewAbsenceRequestConsumer createNewAbsenceRequestConsumer(bool enableWaitlisting, bool forceAccountRecalcBeforeProcessingRequest)
		{
			var resourceCalculator = new ResourceCalculationPrerequisitesLoader(_unitOfWorkFactory,
				new FakeContractScheduleRepository(),
				new FakeActivityRepository(), new FakeAbsenceRepository());

			var requestFactory =
				new RequestFactory(new SwapAndModifyService(new SwapService(), new DoNothingScheduleDayChangeCallBack()),
					new PersonRequestAuthorizationCheckerForTest(), _schedulingResultStateHolder, new FakeGlobalSettingDataRepository());


			var toggleManager = enableWaitlisting
							? new FakeToggleManager(Toggles.Wfm_Requests_Waitlist_36232)
							: new FakeToggleManager();

			if (forceAccountRecalcBeforeProcessingRequest)
			{
				toggleManager.Enable(Toggles.Request_RecalculatePersonAccountBalanceOnRequestConsumer_36850);
			}

			var absenceRequestStatusUpdater = new AbsenceRequestUpdater(new PersonAbsenceAccountProvider(_personAbsenceAccountRepository),
				resourceCalculator,
				new DefaultScenarioFromRepository(_scenarioRepository),
				_loadSchedulingStateHolderForResourceCalculation,
				_loadSchedulesForRequestWithoutResourceCalculation,
				requestFactory,
				new AlreadyAbsentSpecification(_schedulingResultStateHolder),
				new ScheduleIsInvalidSpecification(),
				new PersonRequestCheckAuthorization(),
				new BudgetGroupHeadCountSpecification(_scenarioRepository, _fakeBudgetDayRepository,
					_scheduleProjectionReadOnlyRepository),
				null,
				new BudgetGroupAllowanceSpecification(_schedulingResultStateHolder, _currentScenario, _fakeBudgetDayRepository,
					_scheduleProjectionReadOnlyRepository),
				new FakeScheduleDifferenceSaver(_scheduleRepository),
				_personAccountUpdaterDummy, toggleManager);

			var absenceProcessor = new AbsenceRequestProcessor (absenceRequestStatusUpdater, _scheduleProjectionReadModel, _schedulingResultStateHolder);
			var absenceRequestWaitlistProcessor = new AbsenceRequestWaitlistProcessor (absenceRequestStatusUpdater, _schedulingResultStateHolder, _scheduleProjectionReadModel, new AbsenceRequestWaitlistProvider (_personRequestRepository));
			
			var newAbsenceRequestConsumer = new NewAbsenceRequestConsumer(
				_unitOfWorkFactory, _currentScenario,_personRequestRepository, absenceRequestWaitlistProcessor,absenceProcessor);
			return newAbsenceRequestConsumer;
		}

		private IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, ICurrentScenario currentScenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
				currentScenario.Current(),
				person,
				new DateTimePeriod(startDate, endDate));
		}

		private IPersonAbsence createPersonAbsence(IPerson person, DateTime startDate, DateTime endDate, ICurrentScenario currentScenario)
		{
			return PersonAbsenceFactory.CreatePersonAbsence(
				person, 
				currentScenario.Current(),
				new DateTimePeriod(startDate, endDate));
		}

	}
}