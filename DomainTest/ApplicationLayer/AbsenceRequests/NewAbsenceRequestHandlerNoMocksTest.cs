using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonAbsences;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Absence;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[TestFixture]
	public class NewAbsenceRequestHandlerNoMocksTest
	{
		readonly ICurrentScenario _currentScenario = new FakeCurrentScenario();
		private IPersonRepository _personRepository;
		private IPersonRequestRepository _personRequestRepository;
		private SchedulingResultStateHolder _schedulingResultStateHolder;
		readonly FakeCurrentUnitOfWorkFactory _unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();

		private FakeScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
		private LoadSchedulesForRequestWithoutResourceCalculation _loadSchedulesForRequestWithoutResourceCalculation;
		private LoadSchedulingStateHolderForResourceCalculation _loadSchedulingStateHolderForResourceCalculation;

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
			_scheduleProjectionReadOnlyPersister = new FakeScheduleProjectionReadOnlyPersister();

			_personAccountUpdaterDummy = new PersonAccountUpdaterDummy();

			var skillRepository = new FakeSkillRepository();
			var workloadRepository = new FakeWorkloadRepository();
			var peopleAndSkillLoaderDecider = new PeopleAndSkillLoaderDecider(_personRepository, null);
			var skillDayLoadHelper = new SkillDayLoadHelper(new FakeSkillDayRepository(),
				new MultisiteDayRepository(new FakeUnitOfWork()));
			_scenarioRepository = new FakeScenarioRepository(_currentScenario.Current());

			_loadSchedulesForRequestWithoutResourceCalculation = new LoadSchedulesForRequestWithoutResourceCalculation(_personAbsenceAccountRepository, _scheduleRepository);
			_loadSchedulingStateHolderForResourceCalculation = new LoadSchedulingStateHolderForResourceCalculation(_personRepository, _personAbsenceAccountRepository, skillRepository,
				workloadRepository, _scheduleRepository, peopleAndSkillLoaderDecider, skillDayLoadHelper);
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

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), DateTime.Today, absence, new GrantAbsenceRequest(), true);
			var person = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

			var existingDeniedRequest = createAbsenceRequest(person, absence, requestDateTimePeriod);
			existingDeniedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());

			var newRequest = createAbsenceRequest(person, absence, requestDateTimePeriod);
			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(true, false);
			newAbsenceRequestConsumer.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(existingDeniedRequest.IsApproved);
			Assert.That(existingDeniedRequest.DenyReason, Is.Not.Null.Or.Empty);
			//new request should be denied as is a request for the same day as the accepted absence request
			//now it is denied
			Assert.IsTrue(newRequest.IsDenied && !newRequest.IsWaitlisted);
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
			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(true, false);

			newAbsenceRequestConsumer.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = newRequest.Id.Value });

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
			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(true, false);

			var stopwatch = Stopwatch.StartNew();

			newAbsenceRequestConsumer.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = newRequest.Id.Value });

			stopwatch.Stop();
			Console.WriteLine(stopwatch.Elapsed);

			foreach (var request in personRequests)
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
			existingDeniedRequest.Pending();
			existingDeniedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());  // waitlist
			existingDeniedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());  // deny

			var newRequest = createAbsenceRequest(personTwo, absence, requestDateTimePeriod);
			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(true, false);

			newAbsenceRequestConsumer.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(existingDeniedRequest.IsDenied);
			Assert.IsFalse(existingDeniedRequest.IsAutoDenied);
			Assert.IsFalse(existingDeniedRequest.IsWaitlisted);
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
			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(false, false);
			newAbsenceRequestConsumer.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = newRequest.Id.Value });

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
			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(true, false);

			newAbsenceRequestConsumer.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(newRequest.IsApproved);
			Assert.IsTrue(existingDeniedRequest.IsWaitlisted);
		}

		[Test]
		public void ShouldNotUseWaitlistingWhenWorkflowControlSetIsNotAutoAcceptType()
		{

			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
			var requestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new PendingAbsenceRequest(), true);
			var personOne = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);
			var personTwo = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

			var existingRequest = createAbsenceRequest(personOne, absence, requestDateTimePeriod);
			var newRequest = createAbsenceRequest(personTwo, absence, requestDateTimePeriod);

			var newAbsenceRequestHandler = createNewAbsenceRequestHandler(true, false);
			newAbsenceRequestHandler.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(existingRequest.IsNew);
			Assert.IsTrue(newRequest.IsPending);
		}


		[Test]
		public void ShouldHaveAbsenceAddedEventWhenAbsenceRequestIsGranted()
		{
			var startDateTime = new DateTime(2016,3,1,0,0,0,DateTimeKind.Utc);
			var endDateTime = new DateTime(2016,3,1,23,59,00,DateTimeKind.Utc);
			var requestDateTimePeriod = new DateTimePeriod(startDateTime,endDateTime);
			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016,01,01),new DateTime(2016,12,31),absence,new GrantAbsenceRequest(),false);			

			var person = createAndSetupPerson(startDateTime,endDateTime,workflowControlSet);		
			var newRequest = createAbsenceRequest(person,absence,requestDateTimePeriod);

			var newAbsenceRequestHandler = createNewAbsenceRequestHandler(true,false);
			newAbsenceRequestHandler.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = newRequest.Id.Value });
			
			var scheduleLoadOptions = new ScheduleDictionaryLoadOptions(false,false);
			var schedules = _scheduleRepository.FindSchedulesForPersonOnlyInGivenPeriod(person,scheduleLoadOptions,new DateTimePeriod(startDateTime,endDateTime),_currentScenario.Current());
			var scheduleDay = schedules.SchedulesForDay(new DateOnly(startDateTime)).FirstOrDefault();
			var personAbsence = scheduleDay.PersonAbsenceCollection().SingleOrDefault(abs => abs.Layer.Payload == absence && abs.Person == person);

			var @event = personAbsence.PopAllEvents().Single() as PersonAbsenceAddedEvent;
			@event.Should().Not.Be.Null();
			@event.StartDateTime.Should().Be.EqualTo(startDateTime);
			@event.EndDateTime.Should().Be.EqualTo(endDateTime);
		}

		[Test]
		public void DuplicatedAbsenceRequestShouldGoToDeny()
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
			var requestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var workflowControlSetOne = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new GrantAbsenceRequest(), true);
			var personOne = createAndSetupPerson(startDateTime, endDateTime, workflowControlSetOne);

			var existingWaitlistedRequest = createAbsenceRequest(personOne, absence, requestDateTimePeriod);
			existingWaitlistedRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());

			var newRequest = createAbsenceRequest(personOne, absence, requestDateTimePeriod);
			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(true, false);

			newAbsenceRequestConsumer.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(existingWaitlistedRequest.IsApproved);
			Assert.IsTrue(newRequest.IsDenied && !newRequest.IsWaitlisted);

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
			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(true, false);

			newAbsenceRequestConsumer.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(existingWaitlistedRequest.IsWaitlisted);
			Assert.IsTrue(newRequest.IsApproved);
		}

		[Test]
		public void ShouldNotUseWaitlistingWhenWorkflowControlSetHasMultiOpenPeriodsForAbsenceAndOneIsNotAutoAcceptType()
		{
			var startDateTime = new DateTime(2016, 3, 2, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 6, 23, 59, 00, DateTimeKind.Utc);
			var requestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = true };

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2016, 02, 01), new DateOnly(2016, 03, 3));
			var dateOnlyPeriod2 = new DateOnlyPeriod(new DateOnly(2016, 02, 01), new DateOnly(2016, 03, 10));

			var absenceRequestOpenPeriod1 = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			var absenceRequestOpenPeriod2 = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = new DenyAbsenceRequest(),
				Period = dateOnlyPeriod2,
				OpenForRequestsPeriod = dateOnlyPeriod2
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod2, 0);
			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod1, 1);

			var personOne = PersonFactory.CreatePersonWithGuid("1", "1");
			_personRepository.Add(personOne);
			personOne.WorkflowControlSet = workflowControlSet;

			var personTwo = PersonFactory.CreatePersonWithGuid("2", "2");
			_personRepository.Add(personTwo);
			personTwo.WorkflowControlSet = workflowControlSet;

			createEightHourShiftForDaysInDateCollection(requestDateTimePeriod, personTwo);

			var existingRequest = createAbsenceRequest(personOne, absence, requestDateTimePeriod);
			var newRequest = createAbsenceRequest(personTwo, absence, requestDateTimePeriod);

			var newAbsenceRequestHandler = createNewAbsenceRequestHandler(true, false);
			newAbsenceRequestHandler.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(existingRequest.IsNew); // should not touch this as should not be waitlisting!
			Assert.IsTrue(newRequest.IsDenied);
		}

		[Test]
		public void ShouldAllowDenyReasonToBeUpdatedForWaitlistedRequest()
		{

			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
			var requestDateTimePeriod = new DateTimePeriod(startDateTime, endDateTime);
			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var workflowControlSetOne = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence, new GrantAbsenceRequest(), true);
			var personOne = createAndSetupPerson(startDateTime, endDateTime, workflowControlSetOne);
			var personTwo = createAndSetupPerson(startDateTime, endDateTime, workflowControlSetOne);

			var originalWaitlistedReason = "Original waitlisted reason";

			var existingWaitlistedRequest = createAbsenceRequest(personOne, absence, requestDateTimePeriod);
			existingWaitlistedRequest.Deny(null, originalWaitlistedReason, new PersonRequestAuthorizationCheckerForTest()); //waitlisted request

			var accountDay = new AccountDay(new DateOnly(2016, 3, 1))
			{
				BalanceIn = TimeSpan.FromDays(0),
				Accrued = TimeSpan.FromDays(0),
				Extra = TimeSpan.FromDays(0)
			};

			createPersonAbsenceAccount(personOne, absence, accountDay);

			var accountDay2 = new AccountDay(new DateOnly(2016, 3, 1))
			{
				BalanceIn = TimeSpan.FromDays(0),
				Accrued = TimeSpan.FromDays(1),
				Extra = TimeSpan.FromDays(0)
			};

			createPersonAbsenceAccount(personTwo, absence, accountDay2);


			var newRequest = createAbsenceRequest(personTwo, absence, requestDateTimePeriod);
			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(true, true);

			newAbsenceRequestConsumer.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = newRequest.Id.Value });

			Assert.IsTrue(existingWaitlistedRequest.IsWaitlisted);
			Assert.AreNotEqual(originalWaitlistedReason, existingWaitlistedRequest.DenyReason);
			Assert.IsTrue(newRequest.IsApproved);


		}

		[Test]
		public void ShouldApprovePreviousWaitlistedRequestAfterRemoveAnAbsenceRequest([Values]bool useBudgetGroupAllowanceValidator)
		{
			var date = new DateOnly(2016, 9, 23);
			var budgetGroup = createBudgetGroup(new Dictionary<DateOnly, int> { { date, 1 } });

			var newAbsenceRequestHandler = createNewAbsenceRequestHandler(true, false);

			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
			var workflowControlSet = createBudgetGroupCheckStaffingWorkFlowControlSet(absence, useBudgetGroupAllowanceValidator);

			var person1 = createAndSetupPerson(budgetGroup, workflowControlSet).WithId();
			var request1 = createAbsenceRequest(person1, absence, date.ToDateTimePeriod(person1.PermissionInformation.DefaultTimeZone()));
			newAbsenceRequestHandler.Handle(new NewAbsenceRequestCreatedEvent { PersonRequestId = request1.Id.Value });
			Assert.IsTrue(request1.IsApproved, "request1 is not approved");

			updateReadModel(useBudgetGroupAllowanceValidator, request1);

			var person2 = createAndSetupPerson(budgetGroup, workflowControlSet);
			var request2 = createAbsenceRequest(person2, absence, date.ToDateTimePeriod(person2.PermissionInformation.DefaultTimeZone()));
			newAbsenceRequestHandler.Handle(new NewAbsenceRequestCreatedEvent { PersonRequestId = request2.Id.Value });
			Assert.IsTrue(request2.IsWaitlisted, "request2 is not waitlisted");

			clearReadModel(useBudgetGroupAllowanceValidator, person1);
			createRequestPersonAbsenceRemovedHandler().Handle(new RequestPersonAbsenceRemovedEvent
			{
				PersonRequestId = request1.Id.Value
			});

			Assert.IsTrue(request1.IsCancelled, "request1 is not cancelled");
			Assert.IsTrue(request2.IsApproved, "request2 is not approved");
		}

		private void updateReadModel(bool useBudgetGroupAllowanceValidator, PersonRequest request)
		{
			if (useBudgetGroupAllowanceValidator)
			{
				_scheduleProjectionReadOnlyPersister.AddActivity(
					new ScheduleProjectionReadOnlyModel
					{
						BelongsToDate = new DateOnly(request.Request.Period.StartDateTime),
						PayloadId = (request.Request as IAbsenceRequest).Absence.Id.Value,
						PersonId = request.Person.Id.Value,
						ScenarioId = _currentScenario.Current().Id.Value,
						StartDateTime = request.Request.Period.StartDateTime,
						EndDateTime = request.Request.Period.EndDateTime,
						ContractTime = TimeSpan.FromHours(8)
					});
			}
			else
			{
				_scheduleProjectionReadOnlyPersister.SetNumberOfAbsencesPerDayAndBudgetGroup(1);
			}
		}

		private void clearReadModel(bool useBudgetGroupAllowanceValidator, IPerson person)
		{
			if (useBudgetGroupAllowanceValidator)
			{
				_scheduleProjectionReadOnlyPersister.Clear(person.Id.Value);
			}
			else
			{
				_scheduleProjectionReadOnlyPersister.SetNumberOfAbsencesPerDayAndBudgetGroup(0);
			}
		}

		[Test]
		public void ShouldApproveRequestAgainAfterRemoveAnAbsenceRequest([Values]bool useBudgetGroupAllowanceValidator)
		{
			var date = new DateOnly(2016, 9, 23);
			var budgetGroup = createBudgetGroup(new Dictionary<DateOnly, int> { { date, 1 } });

			var newAbsenceRequestHandler = createNewAbsenceRequestHandler(true, false);

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createBudgetGroupCheckStaffingWorkFlowControlSet(absence, useBudgetGroupAllowanceValidator);

			var person1 = createAndSetupPerson(budgetGroup, workflowControlSet);
			var request1 = createAbsenceRequest(person1, absence, date.ToDateTimePeriod(person1.PermissionInformation.DefaultTimeZone()));
			newAbsenceRequestHandler.Handle(new NewAbsenceRequestCreatedEvent { PersonRequestId = request1.Id.Value });
			Assert.IsTrue(request1.IsApproved, "request1 is not approved");

			createRequestPersonAbsenceRemovedHandler().Handle(new RequestPersonAbsenceRemovedEvent
			{
				PersonRequestId = request1.Id.Value
			});
			Assert.IsTrue(request1.IsCancelled, "request1 is not cancelled");

			var request2 = createAbsenceRequest(person1, absence, date.ToDateTimePeriod(person1.PermissionInformation.DefaultTimeZone()));
			newAbsenceRequestHandler.Handle(new NewAbsenceRequestCreatedEvent { PersonRequestId = request2.Id.Value });
			Assert.IsTrue(request2.IsApproved, "request2 is not approved");
		}

		[Test]
		public void ShouldApproveOneDayRequestAfterTwoDaysRequestFailed([Values]bool useBudgetGroupAllowanceValidator)
		{
			var date1 = new DateOnly(2016, 9, 23);
			var date2 = new DateOnly(2016, 9, 24);
			var budgetGroup = createBudgetGroup(new Dictionary<DateOnly, int> { { date1, 1}, { date2, 0 } });

			var newAbsenceRequestHandler = createNewAbsenceRequestHandler(true, false);

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createBudgetGroupCheckStaffingWorkFlowControlSet(absence, useBudgetGroupAllowanceValidator);

			var person1 = createAndSetupPerson(budgetGroup, workflowControlSet);
			var request1 = createAbsenceRequest(person1, absence,
				new DateTimePeriod(
					new DateTime(date1.Year, date1.Month, date1.Day, 0, 0, 0, DateTimeKind.Utc),
					new DateTime(date2.Year, date2.Month, date2.Day, 0, 0, 0, DateTimeKind.Utc)));
			newAbsenceRequestHandler.Handle(new NewAbsenceRequestCreatedEvent { PersonRequestId = request1.Id.Value });
			Assert.IsTrue(request1.IsWaitlisted, "request1 is not waitlisted");

			var request2 = createAbsenceRequest(person1, absence, date1.ToDateTimePeriod(person1.PermissionInformation.DefaultTimeZone()));
			newAbsenceRequestHandler.Handle(new NewAbsenceRequestCreatedEvent { PersonRequestId = request2.Id.Value });
			Assert.IsTrue(request2.IsApproved, "request2 is not approved");
		}

		private BudgetGroup createBudgetGroup(Dictionary<DateOnly, int> dateAllowances)
		{
			var budgetGroup = new BudgetGroup {Name = "budgetGroup1"};
			foreach (var dateAllowance in dateAllowances)
			{
				var budgetDay = new BudgetDay(budgetGroup, _currentScenario.Current(), dateAllowance.Key)
				{
					OvertimeHours = 1,
					TotalAllowance = dateAllowance.Value,
					Allowance = dateAllowance.Value,
					FulltimeEquivalentHours = 8
				};
				_fakeBudgetDayRepository.Add(budgetDay);
			}
			return budgetGroup;
		}

		private void createPersonAbsenceAccount(IPerson person, IAbsence absence, IAccount accountDay)
		{
			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
			personAbsenceAccount.Absence.Tracker = Tracker.CreateDayTracker();
			personAbsenceAccount.Add(accountDay);

			var personAccountCollection = new PersonAccountCollection(person) { personAbsenceAccount };
			_schedulingResultStateHolder.AllPersonAccounts = new Dictionary<IPerson, IPersonAccountCollection>
			{
				{person, personAccountCollection}
			};

			_personAbsenceAccountRepository.Add(personAbsenceAccount);
		}

		private void createEightHourShiftForDaysInDateCollection(DateTimePeriod requestDateTimePeriod, IPerson personOne)
		{
			foreach (var day in requestDateTimePeriod.ToDateOnlyPeriod(personOne.PermissionInformation.DefaultTimeZone()).DayCollection())
			{
				var assignment = createAssignment(personOne, DateTime.SpecifyKind(day.Date.AddHours(8), DateTimeKind.Utc), DateTime.SpecifyKind(day.Date.AddHours(17), DateTimeKind.Utc), _currentScenario);
				_scheduleRepository.Set(new[] { assignment });
			}
		}

		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			var personRequest = new PersonRequest(person, new Domain.AgentInfo.Requests.AbsenceRequest(absence, requestDateTimePeriod));

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

			var newAbsenceRequestConsumer = createNewAbsenceRequestHandler(false, forcePersonalAccountUpdate);
			var request = createAbsenceRequest(person, absence, new DateTimePeriod(startDateTime, endDateTime));

			newAbsenceRequestConsumer.Handle(new NewAbsenceRequestCreatedEvent() { PersonRequestId = request.Id.Value });

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

		private IPerson createAndSetupPerson(IBudgetGroup budgetGroup, IWorkflowControlSet workflowControlSet)
		{
			var personPeriodDateOnly = new DateOnly(2016, 1, 1);
			var person = PersonFactory.CreatePersonWithPersonPeriod(personPeriodDateOnly);
			person.PersonPeriods(personPeriodDateOnly.ToDateOnlyPeriod()).FirstOrDefault().BudgetGroup = budgetGroup;
			person.WorkflowControlSet = workflowControlSet;
			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence, IProcessAbsenceRequest processAbsenceRequest, bool waitlistingIsEnabled)
		{
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = waitlistingIsEnabled };
			workflowControlSet.SetId(Guid.NewGuid());

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				AbsenceRequestProcess = processAbsenceRequest,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;

		}

		private static WorkflowControlSet createBudgetGroupCheckStaffingWorkFlowControlSet(IAbsence absence, bool useBudgetGroupAllowanceValidator)
		{
			var workflowControlSet = WorkflowControlSetFactory.CreateWorkFlowControlSet(absence, new GrantAbsenceRequest(), true);
			var absenceRequestOpenPeriod = workflowControlSet.AbsenceRequestOpenPeriods.FirstOrDefault();
			if (useBudgetGroupAllowanceValidator)
			{
				absenceRequestOpenPeriod.StaffingThresholdValidator = new BudgetGroupAllowanceValidator();
			}
			else
			{
				absenceRequestOpenPeriod.StaffingThresholdValidator = new BudgetGroupHeadCountValidator();
			}
			return workflowControlSet;
		}

		private NewAbsenceRequestHandler createNewAbsenceRequestHandler(bool enableWaitlisting, bool forceAccountRecalcBeforeProcessingRequest)
		{
			var absenceRequestStatusUpdater = createAbsenceRequestUpdater(enableWaitlisting, forceAccountRecalcBeforeProcessingRequest);

			var absenceProcessor = new AbsenceRequestProcessor(absenceRequestStatusUpdater, () => _schedulingResultStateHolder);
			var absenceRequestWaitlistProcessor = new AbsenceRequestWaitlistProcessor(absenceRequestStatusUpdater, () => _schedulingResultStateHolder, new AbsenceRequestWaitlistProvider(_personRequestRepository));

			var newAbsenceRequestConsumer = new NewAbsenceRequestHandler(
				_unitOfWorkFactory, _currentScenario, _personRequestRepository, absenceRequestWaitlistProcessor, absenceProcessor);
			return newAbsenceRequestConsumer;
		}

		private AbsenceRequestUpdater createAbsenceRequestUpdater(bool enableWaitlisting, bool forceAccountRecalcBeforeProcessingRequest)
		{
			var toggleManager = enableWaitlisting
				? new FakeToggleManager(Toggles.Wfm_Requests_Waitlist_36232)
				: new FakeToggleManager();

			if (forceAccountRecalcBeforeProcessingRequest)
			{
				toggleManager.Enable(Toggles.Request_RecalculatePersonAccountBalanceOnRequestConsumer_36850);
			}

			var requestFactory =
				new RequestFactory(new SwapAndModifyService(new SwapService(), new DoNothingScheduleDayChangeCallBack()),
				new PersonRequestAuthorizationCheckerForTest(), new FakeGlobalSettingDataRepository(), null, new DoNothingScheduleDayChangeCallBack());

			var resourceCalculator = new ResourceCalculationPrerequisitesLoader(_unitOfWorkFactory,
				new FakeContractScheduleRepository(),
				new FakeActivityRepository(), new FakeAbsenceRepository());

			var absenceRequestStatusUpdater =
				new AbsenceRequestUpdater(new PersonAbsenceAccountProvider(_personAbsenceAccountRepository),
					resourceCalculator,
					new DefaultScenarioFromRepository(_scenarioRepository),
					_loadSchedulingStateHolderForResourceCalculation,
					_loadSchedulesForRequestWithoutResourceCalculation,
					requestFactory,
					new AlreadyAbsentSpecification(),
					new ScheduleIsInvalidSpecification(),
					new PersonRequestCheckAuthorization(),
					new BudgetGroupHeadCountSpecification(_scenarioRepository, _fakeBudgetDayRepository,
						_scheduleProjectionReadOnlyPersister),
					null,
					new BudgetGroupAllowanceSpecification(_currentScenario, _fakeBudgetDayRepository,
						_scheduleProjectionReadOnlyPersister),
					new FakeScheduleDifferenceSaver(_scheduleRepository),
					_personAccountUpdaterDummy, toggleManager);
			return absenceRequestStatusUpdater;
		}

		private IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, ICurrentScenario currentScenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
				currentScenario.Current(),
				person,
				new DateTimePeriod(startDate, endDate));
		}

		private RequestPersonAbsenceRemovedHandler createRequestPersonAbsenceRemovedHandler()
		{
			var absenceRequestStatusUpdater = createAbsenceRequestUpdater(true, false);
			var absenceRequestWaitlistProcessor = new AbsenceRequestWaitlistProcessor(absenceRequestStatusUpdater
				, () => _schedulingResultStateHolder, new AbsenceRequestWaitlistProvider(_personRequestRepository));
			var absenceRequestCancelService = new AbsenceRequestCancelService(new PersonRequestAuthorizationCheckerConfigurable(), _currentScenario);
			var requestPersonAbsenceRemovedHandler = new RequestPersonAbsenceRemovedHandler(_unitOfWorkFactory, absenceRequestWaitlistProcessor
				, _personRequestRepository, absenceRequestCancelService);
			return requestPersonAbsenceRemovedHandler;
		}

	}
}