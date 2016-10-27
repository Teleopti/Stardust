﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[TestWithStaticDependenciesAvoidUse]
	class ApproveRequestCommandHandlerTest
	{
		private FakeCurrentScenario _scenario;
		private FakePersonRequestRepository _personRequestRepository;
		private FakePersonAbsenceAccountRepository _personAbsenceAccountRepository;
		private FakeScheduleStorage _fakeScheduleStorage;
		private FakeScheduleDifferenceSaver _scheduleDifferenceSaver;
		private RequestApprovalServiceFactory _requestApprovalServiceFactory;
		private ApproveRequestCommandHandler _approveRequestCommandHandler;

		[SetUp]
		public void Setup()
		{
			_scenario = new FakeCurrentScenario();
			_personRequestRepository = new FakePersonRequestRepository();
			_personAbsenceAccountRepository = new FakePersonAbsenceAccountRepository();

			_fakeScheduleStorage = new FakeScheduleStorage();
			_scheduleDifferenceSaver = new FakeScheduleDifferenceSaver(_fakeScheduleStorage);

			var businessRules = new BusinessRulesForPersonalAccountUpdate (_personAbsenceAccountRepository, new SchedulingResultStateHolder());

			_requestApprovalServiceFactory = new RequestApprovalServiceFactory(
				new SwapAndModifyService(null, null), 
				new FakeGlobalSettingDataRepository(),
				businessRules, new CheckingPersonalAccountDaysProvider(_personAbsenceAccountRepository), new DoNothingScheduleDayChangeCallBack()
			);
			
			var writeProtectedScheduleCommandValidator = new WriteProtectedScheduleCommandValidator ( 
				new ConfigurablePermissions(), new FakeCommonAgentNameProvider(), new FakeLoggedOnUser(), new SwedishCulture());

			_approveRequestCommandHandler = new ApproveRequestCommandHandler(
				_fakeScheduleStorage, _scheduleDifferenceSaver,
				new PersonRequestAuthorizationCheckerForTest(),
				new DifferenceEntityCollectionService<IPersistableScheduleData>(),
				_personRequestRepository, _requestApprovalServiceFactory,
				_scenario, writeProtectedScheduleCommandValidator );
		}

		[Test]
		public void ShouldUpdatePersonalAccountWhenRequestIsGranted()
		{
			var absenceDateTimePeriod = new DateTimePeriod (2016, 01, 01, 00, 2016, 01, 01, 23);

			var person = PersonFactory.CreatePersonWithId();
			var absence = new Absence();

			var accountDay = createAccountDay(person, absence, new DateOnly(2015, 12, 1));

			addAssignment(person, absenceDateTimePeriod);
			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);

			_approveRequestCommandHandler.Handle(new ApproveRequestCommand(){ PersonRequestId = personRequest.Id.Value });

			Assert.IsTrue(personRequest.IsApproved);
			Assert.AreEqual (24, accountDay.Remaining.TotalDays);
		}

		[Test]
		public void ShouldPublishAbsenceAddedEventWhenRequestIsGranted()
		{
			var absenceDateTimePeriod = new DateTimePeriod(2016,01,01,00,2016,01,01,23);

			var person = PersonFactory.CreatePersonWithId();
			var absence = new Absence();

			var accountDay = createAccountDay(person,absence,new DateOnly(2015,12,1));

			addAssignment(person,absenceDateTimePeriod);
			var personRequest = createAbsenceRequest(person,absence,absenceDateTimePeriod);

			_approveRequestCommandHandler.Handle(new ApproveRequestCommand() { PersonRequestId = personRequest.Id.Value });

			var personAbsence = _approveRequestCommandHandler.GetRequestApprovalService().GetApprovedPersonAbsence();

			var @events = personAbsence.PopAllEvents();
			@events.Single().Should().Be.OfType<PersonAbsenceAddedEvent>();
		}


		[Test]
		public void ShouldUpdateMessageWhenThereIsAReplyMessage()
		{
			var absenceDateTimePeriod = new DateTimePeriod(2016, 01, 01, 00, 2016, 01, 01, 23);

			var person = PersonFactory.CreatePersonWithId();
			var absence = new Absence();

			createAccountDay(person, absence, new DateOnly(2015, 12, 1));

			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);
			var messagePropertyChanged = false;
			personRequest.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName.Equals("Message", StringComparison.OrdinalIgnoreCase))
				{
					messagePropertyChanged = true;
				}
			};
			var command = new ApproveRequestCommand() {PersonRequestId = personRequest.Id.Value, ReplyMessage = "test"};

			_approveRequestCommandHandler.Handle(command);

			Assert.IsTrue(personRequest.IsApproved);
			Assert.IsTrue(personRequest.GetMessage(new NoFormatting()).Contains("test"));
			Assert.IsTrue(messagePropertyChanged);
			Assert.IsTrue(command.IsReplySuccess);
		}

		[Test]
		public void ApproveDeniedRequestShouldNotUpdatePersonalAccount()
		{
			var person = PersonFactory.CreatePersonWithId();
			var absence = AbsenceFactory.CreateAbsenceWithId();

			var balance = TimeSpan.FromDays(7);
			var accountDay = createAccountDay(person, absence, new DateOnly(2015, 12, 1), balance);

			var absenceDateTimePeriod = new DateTimePeriod(2016, 01, 01, 00, 2016, 01, 01, 23);
			addAssignment(person, absenceDateTimePeriod);

			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);
			personRequest.Deny(null, "test", new PersonRequestAuthorizationCheckerConfigurable());

			var command = new ApproveRequestCommand() { PersonRequestId = personRequest.Id.Value };
			_approveRequestCommandHandler.Handle(command);

			Assert.IsTrue(command.ErrorMessages.Contains("A request that is Denied cannot be Approved."));
			Assert.IsTrue(personRequest.IsDenied);
			Assert.IsTrue(accountDay.LatestCalculatedBalance.Equals(balance));
		}

		[Test]
		public void ApproveCancelledRequestShouldNotUpdatePersonalAccount()
		{
			var person = PersonFactory.CreatePersonWithId();
			var absence = AbsenceFactory.CreateAbsenceWithId();

			var balance = TimeSpan.FromDays(7);
			var accountDay = createAccountDay(person, absence, new DateOnly(2015, 12, 1), balance);
			
			var absenceDateTimePeriod = new DateTimePeriod(2016, 01, 01, 00, 2016, 01, 01, 23);
			addAssignment(person, absenceDateTimePeriod);

			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);
			var personRequestCheckAuthorization = new PersonRequestAuthorizationCheckerConfigurable();
			personRequest.Approve(new ApprovalServiceForTest(), personRequestCheckAuthorization);
			personRequest.Cancel(personRequestCheckAuthorization);

			var command = new ApproveRequestCommand() { PersonRequestId = personRequest.Id.Value };
			_approveRequestCommandHandler.Handle(command);

			Assert.IsTrue(command.ErrorMessages.Contains("A request that is Cancelled cannot be Approved."));
			Assert.IsTrue(personRequest.IsCancelled);
			Assert.IsTrue(accountDay.LatestCalculatedBalance.Equals(balance));
		}

		private PersonRequest createAbsenceRequest (IPerson person, IAbsence absence, DateTimePeriod dateTimePeriod)
		{
			var personRequestFactory = new PersonRequestFactory() {Person = person};
			var absenceRequest = personRequestFactory.CreateAbsenceRequest (absence,dateTimePeriod);
			var personRequest = absenceRequest.Parent as PersonRequest;
			personRequest.SetId (Guid.NewGuid());

			_personRequestRepository.Add (personRequest);

			return personRequest;
		}

		private AccountDay createAccountDay(IPerson person, IAbsence absence, DateOnly startDate, TimeSpan? balance = null)
		{
			var accountDay = new AccountDay(startDate)
			{
				BalanceIn = TimeSpan.FromDays(5),
				Accrued = TimeSpan.FromDays(20),
				Extra = TimeSpan.FromDays(0),
				LatestCalculatedBalance = balance.GetValueOrDefault(TimeSpan.Zero)
			};

			var personAbsenceAccount = new PersonAbsenceAccount(person, absence);
			personAbsenceAccount.Absence.Tracker = Tracker.CreateDayTracker();
			personAbsenceAccount.Add(accountDay);

			_personAbsenceAccountRepository.Add(personAbsenceAccount);
			return accountDay;
		}

		private void addAssignment(IPerson person, DateTimePeriod absenceDateTimePeriod)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario.Current(), person,
				absenceDateTimePeriod);
			_fakeScheduleStorage.Add(assignment);
		}
	}
}
