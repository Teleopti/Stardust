using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	[DomainTest]
	public class ApproveRequestCommandHandlerTest : IIsolateSystem
	{
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceAccountRepository PersonAbsenceAccountRepository;
		public ApproveRequestCommandHandler Target;

		[Test]
		public void ShouldUpdatePersonalAccountWhenRequestIsGranted()
		{
			var absenceDateTimePeriod = new DateTimePeriod(2016, 01, 01, 00, 2016, 01, 01, 23);

			var person = PersonFactory.CreatePersonWithId();
			var absence = new Absence();

			var accountDay = createAccountDay(person, absence, new DateOnly(2015, 12, 1));

			addAssignment(person, ScenarioRepository.Has("Default"), absenceDateTimePeriod);
			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);

			Target.Handle(new ApproveRequestCommand {PersonRequestId = personRequest.Id.Value});

			Assert.IsTrue(personRequest.IsApproved);
			Assert.AreEqual(24, accountDay.Remaining.TotalDays);
		}

		[Test]
		public void ShouldPublishAbsenceAddedEventWhenRequestIsGranted()
		{
			var absenceDateTimePeriod = new DateTimePeriod(2016, 01, 01, 00, 2016, 01, 01, 23);

			var person = PersonFactory.CreatePersonWithId();
			var absence = new Absence();

			createAccountDay(person, absence, new DateOnly(2015, 12, 1));

			addAssignment(person, ScenarioRepository.Has("Default"), absenceDateTimePeriod);
			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);

			Target.Handle(new ApproveRequestCommand
				{PersonRequestId = personRequest.Id.Value});

			var personAbsence = ((IAbsenceApprovalService) Target.GetRequestApprovalService())
				.GetApprovedPersonAbsence();

			var @events = personAbsence.PopAllEvents(null);
			@events.Single().Should().Be.OfType<PersonAbsenceAddedEvent>();
		}


		[Test]
		public void ShouldUpdateMessageWhenThereIsAReplyMessage()
		{
			var absenceDateTimePeriod = new DateTimePeriod(2016, 01, 01, 00, 2016, 01, 01, 23);

			var person = PersonFactory.CreatePersonWithId();
			var absence = new Absence();
			ScenarioRepository.Has("Default");

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
			var command = new ApproveRequestCommand {PersonRequestId = personRequest.Id.Value, ReplyMessage = "test"};

			Target.Handle(command);

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
			addAssignment(person, ScenarioRepository.Has("Default"), absenceDateTimePeriod);

			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);
			personRequest.Deny("test", new PersonRequestAuthorizationCheckerConfigurable());

			var command = new ApproveRequestCommand {PersonRequestId = personRequest.Id.Value};
			Target.Handle(command);

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
			addAssignment(person, ScenarioRepository.Has("Default"), absenceDateTimePeriod);

			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);
			var personRequestCheckAuthorization = new PersonRequestAuthorizationCheckerConfigurable();
			personRequest.Approve(new ApprovalServiceForTest(), personRequestCheckAuthorization);
			personRequest.Cancel(personRequestCheckAuthorization);

			var command = new ApproveRequestCommand {PersonRequestId = personRequest.Id.Value};
			Target.Handle(command);

			Assert.IsTrue(command.ErrorMessages.Contains("A request that is Cancelled cannot be Approved."));
			Assert.IsTrue(personRequest.IsCancelled);
			Assert.IsTrue(accountDay.LatestCalculatedBalance.Equals(balance));
		}

		[Test]
		public void ShouldNotBeAbleToApproveDeletedRequest()
		{
			var person = PersonFactory.CreatePersonWithId();
			var absence = AbsenceFactory.CreateAbsenceWithId();

			var absenceDateTimePeriod = new DateTimePeriod(2016, 01, 01, 00, 2016, 01, 01, 23);
			addAssignment(person, ScenarioRepository.Has("Default"), absenceDateTimePeriod);

			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);

			personRequest.SetDeleted();

			var command = new ApproveRequestCommand {PersonRequestId = personRequest.Id.Value};
			Target.Handle(command);
			command.ErrorMessages.Single().Should().Be.EqualTo(UserTexts.Resources.RequestHasBeenDeleted);
		}

		[Test]
		public void ApproveApprovedRequestShouldNotUpdatePersonalAccount()
		{
			var person = PersonFactory.CreatePersonWithId();
			var absence = AbsenceFactory.CreateAbsenceWithId();

			var balance = TimeSpan.FromDays(7);
			var accountDay = createAccountDay(person, absence, new DateOnly(2015, 12, 1), balance);

			var absenceDateTimePeriod = new DateTimePeriod(2016, 01, 01, 00, 2016, 01, 01, 23);
			addAssignment(person, ScenarioRepository.Has("Default"), absenceDateTimePeriod);

			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);
			var personRequestCheckAuthorization = new PersonRequestAuthorizationCheckerConfigurable();
			personRequest.Approve(new ApprovalServiceForTest(), personRequestCheckAuthorization);

			var command = new ApproveRequestCommand {PersonRequestId = personRequest.Id.Value};
			Target.Handle(command);

			Assert.IsTrue(personRequest.IsApproved);
			Assert.IsTrue(accountDay.LatestCalculatedBalance.Equals(balance));
		}

		[Test]
		public void ShouldApproveTextRequest()
		{
			ScenarioRepository.Has("Default");
			var personRequestFactory = new PersonRequestFactory {Person = PersonFactory.CreatePerson()};
			var personRequest = personRequestFactory.CreatePersonRequest();
			var textRequest = new TextRequest(DateOnly.Today.ToDateTimePeriod(TimeZoneInfo.Utc));
			personRequest.Request = textRequest;
			personRequest.SetId(Guid.NewGuid());
			PersonRequestRepository.Add(personRequest);

			var command = new ApproveRequestCommand {PersonRequestId = personRequest.Id.Value};
			Target.Handle(command);

			Assert.IsTrue(personRequest.IsApproved);
		}

		[Test]
		public void ShouldSkipAlreadyApprovedRequest()
		{
			var person = PersonFactory.CreatePersonWithId();
			var absence = AbsenceFactory.CreateAbsenceWithId();

			var balance = TimeSpan.FromDays(7);
			var accountDay = createAccountDay(person, absence, new DateOnly(2015, 12, 1), balance);

			var absenceDateTimePeriod = new DateTimePeriod(2016, 01, 01, 00, 2016, 01, 01, 23);
			addAssignment(person, ScenarioRepository.Has("Default"), absenceDateTimePeriod);

			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod);
			var personRequestCheckAuthorization = new PersonRequestAuthorizationCheckerConfigurable();
			personRequest.Approve(new ApprovalServiceForTest(), personRequestCheckAuthorization);

			var command = new ApproveRequestCommand {PersonRequestId = personRequest.Id.Value};
			Target.Handle(command);

			Assert.IsTrue(command.ErrorMessages.IsEmpty());
			Assert.IsTrue(personRequest.IsApproved);
			Assert.IsTrue(accountDay.LatestCalculatedBalance.Equals(balance));
		}

		[Test]
		public void ShouldLoadScheduleForOvertimeRequest()
		{
			var skill1 = new Domain.Forecasting.Skill("skill1");
			var baseDate = new DateOnly(2016, 12, 1);
			var person = PersonFactory.CreatePersonWithPersonPeriod(baseDate, new[] {skill1});
			person.WorkflowControlSet = new WorkflowControlSet();

			var personRequestFactory = new PersonRequestFactory {Person = person};
			var personRequest = personRequestFactory.CreatePersonRequest();
			var overtimePeriod = DateOnly.Today.ToDateTimePeriod(TimeZoneInfo.Utc);
			var overtimeRequest =
				new OvertimeRequest(new MultiplicatorDefinitionSet("test", MultiplicatorType.Overtime), overtimePeriod);
			personRequest.Request = overtimeRequest;
			personRequest.SetId(Guid.NewGuid());
			PersonRequestRepository.Add(personRequest);

			addAssignment(person, ScenarioRepository.Has("Default"), overtimePeriod);

			var command = new ApproveRequestCommand {PersonRequestId = personRequest.Id.Value};
			Target.Handle(command);

			var schedules = PersonAssignmentRepository.LoadAll();
			Assert.IsTrue(schedules.Count() == 1);
		}

		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod dateTimePeriod)
		{
			var personRequestFactory = new PersonRequestFactory() {Person = person};
			var absenceRequest = personRequestFactory.CreateAbsenceRequest(absence, dateTimePeriod);
			var personRequest = absenceRequest.Parent as PersonRequest;
			personRequest.SetId(Guid.NewGuid());

			PersonRequestRepository.Add(personRequest);

			return personRequest;
		}

		private AccountDay createAccountDay(IPerson person, IAbsence absence, DateOnly startDate,
			TimeSpan? balance = null)
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

			PersonAbsenceAccountRepository.Add(personAbsenceAccount);
			return accountDay;
		}

		private void addAssignment(IPerson person, IScenario scenario, DateTimePeriod absenceDateTimePeriod)
		{
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person,
				scenario, absenceDateTimePeriod);
			PersonAssignmentRepository.Add(assignment);
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ApproveRequestCommandHandler>().For<IHandleCommand<ApproveRequestCommand>>();
		}
	}
}
