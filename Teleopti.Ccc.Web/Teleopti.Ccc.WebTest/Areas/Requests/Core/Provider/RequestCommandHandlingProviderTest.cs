using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.Provider
{
#pragma warning disable 0649

	[TestFixture, DomainTest]
	public class RequestCommandHandlingProviderTest : ISetup
	{
		public IRequestCommandHandlingProvider Target;
		public MutableNow Now;
		public FakePersonRequestRepository PersonRequestRepository;
		public ICurrentScenario Scenario;
		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public ILoggedOnUser LoggedOnUser;
		public IUserCulture UserCulture;
		public ICommonAgentNameProvider CommonAgentNameProvider;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public ScheduleStorage ScheduleStorage;
		public PersonRequestAuthorizationCheckerConfigurable PersonRequestCheckAuthorization;
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public FakePersonAbsenceAccountRepositoryWithOptimisticLockException PersonAbsenceAccountRepository;
		public FullPermission Permission;
		private IAbsence _absence;
		

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakePersonAbsenceAccountRepositoryWithOptimisticLockException>()
				.For<IPersonAbsenceAccountRepository>();
			system.UseTestDouble<PersonRequestAuthorizationCheckerConfigurable>().For<IPersonRequestCheckAuthorization>();
			system.UseTestDouble(new FakeScenarioRepository(new Scenario {DefaultScenario = true})).For<IScenarioRepository>();
			system.UseTestDouble<PersonAbsenceRemover>().For<IPersonAbsenceRemover>();
			system.UseTestDouble<RequestCommandHandlingProvider>().For<IRequestCommandHandlingProvider>();
			system.UseTestDouble<SaveSchedulePartService>().For<ISaveSchedulePartService>();
			system.UseTestDouble<PersonAbsenceCreator>().For<IPersonAbsenceCreator>();
			_absence = AbsenceFactory.CreateAbsence("Holiday").WithId();
		}

		[Test]
		public void TargetShouldNotBeNull()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotHandleApproveCommandWithInvalidRequestId()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var result = Target.ApproveRequests(new List<Guid> { new Guid() }, string.Empty);
			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldApproveAbsenceRequestWithPendingStatus()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("tester");

			var absence = AbsenceFactory.CreateAbsence("absence");
			var personRequest = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			personRequest.Pending();

			var result = Target.ApproveRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, string.Empty);

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(1);
			personRequest.IsApproved.Should().Be(true);
		}

		[Test]
		public void ShouldApproveAbsenceRequestWithReplyMessage()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("tester");

			var absence = AbsenceFactory.CreateAbsence("absence");
			var personRequest = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			personRequest.Pending();

			var result = Target.ApproveRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, "test");

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(1);
			personRequest.IsApproved.Should().Be(true);
			personRequest.GetMessage(new NoFormatting()).Should().Contain("test");
		}

		[Test]
		public void ShouldApproveAllAbsenceRequestsWithPendingStatus()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("tester").WithId();

			var absence = AbsenceFactory.CreateAbsence("absence").WithId();

			var personRequest1 = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var personRequest2 = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));

			personRequest1.Pending();
			personRequest2.Pending();

			var result = Target.ApproveRequests(new List<Guid>
			{
				personRequest1.Id.GetValueOrDefault(),
				personRequest2.Id.GetValueOrDefault()
			}, string.Empty);

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSendCommandForApproveWithValidators()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var requestIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

			var result = Target.ApproveWithValidators(requestIds, RequestValidatorsFlag.BudgetAllotmentValidator);

			result.AffectedRequestIds.Count().Should().Be(0);
			result.ErrorMessages.Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotHandleDenyCommandWithInvalidRequestId()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var result = Target.DenyRequests(new List<Guid> { new Guid() }, string.Empty);
			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldDenyAbsenceRequestWithPendingStatus()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");

			var personRequest = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			personRequest.Pending();

			var result = Target.DenyRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, string.Empty);

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldDenyAbsenceRequestWithReplyMessage()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");

			var personRequest = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			personRequest.Pending();

			var result = Target.DenyRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, "test");

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(1);
			personRequest.IsDenied.Should().Be(true);
			personRequest.GetMessage(new NoFormatting()).Should().Contain("test");
		}

		[Test]
		public void ShouldDenyAllAbsenceRequestsWithPendingStatus()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");
			var personRequest1 = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var personRequest2 = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			personRequest1.Pending();
			personRequest2.Pending();

			var result = Target.DenyRequests(new List<Guid>
			{
				personRequest1.Id.GetValueOrDefault(),
				personRequest2.Id.GetValueOrDefault()
			}, string.Empty);

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldManuallyDenyWaitlistRequest()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");

			person.WorkflowControlSet = createWorkFlowControlSet(new DateTime(2016, 2, 1, 10, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 4, 1, 23, 00, 00, DateTimeKind.Utc), absence, true);
			var waitlistedPersonRequest = createWaitlistedAbsenceRequest(person, absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			Target.DenyRequests(new List<Guid> { waitlistedPersonRequest.Id.GetValueOrDefault() }, string.Empty);
			waitlistedPersonRequest.IsWaitlisted.Should().Be.False();
			waitlistedPersonRequest.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldManuallyApproveWaitlistedRequests()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");

			person.WorkflowControlSet = createWorkFlowControlSet(new DateTime(2016, 2, 1, 10, 0, 0, DateTimeKind.Utc),
				DateTime.Today, absence, true);
			var dateTimePeriod = new DateTimePeriod(new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc));

			var waitlistedPersonRequest = createWaitlistedAbsenceRequest(person, absence, dateTimePeriod);

			Target.ApproveRequests(new List<Guid> { waitlistedPersonRequest.Id.GetValueOrDefault() }, string.Empty);

			waitlistedPersonRequest.IsWaitlisted.Should().Be.False();
			waitlistedPersonRequest.IsDenied.Should().Be.False();
			waitlistedPersonRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldReturnWriteProtectedMsgWhenAttemptingToApproveAnAbsenceWhereScheduleIsWriteProtected()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var writeProtectErrorMessage = getWriteProtectMessage(person, dateTimePeriod.StartDateTime);

			Permission.AddToBlackList(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule);
			var result = doApproveAbsenceWriteProtectedTest(person, dateTimePeriod);

			result.AffectedRequestIds.Count().Should().Be.EqualTo(0);
			result.ErrorMessages.Contains(writeProtectErrorMessage).Should().Be.True();
		}

		[Test]
		public void ShouldNotReturnWriteProtectedMsgWhenAttemptingToApproveAnAbsenceWhereScheduleIsWriteProtected()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var writeProtectErrorMessage = getWriteProtectMessage(person, dateTimePeriod.StartDateTime);

			var result = doApproveAbsenceWriteProtectedTest(person, dateTimePeriod);
			result.ErrorMessages.Contains(writeProtectErrorMessage).Should().Be.False();
		}

		[Test]
		public void ShouldReturnWriteProtectedMsgWhenAttemptingToCancelAnAbsenceWhereScheduleIsWriteProtected()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var writeProtectErrorMessage = getWriteProtectMessage(person, dateTimePeriod.StartDateTime);

			Permission.AddToBlackList(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule);
			var result = doCancelAbsenceWriteProtectedTest(person, dateTimePeriod);

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(0);
			result.ErrorMessages.Contains(writeProtectErrorMessage).Should().Be.True();
		}

		[Test]
		public void ShouldNotReturnWriteProtectedMsgWhenAttemptingToCancelAnAbsenceWhereScheduleIsWriteProtected()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var writeProtectErrorMessage = getWriteProtectMessage(person, dateTimePeriod.StartDateTime);

			PersonRequestCheckAuthorization.HasCancelPermission = false;

			var result = doCancelAbsenceWriteProtectedTest(person, dateTimePeriod);
			result.ErrorMessages.Contains(writeProtectErrorMessage).Should().Be.False();
		}

		[Test]
		public void ShouldNotCancelPersonRequestWhenNoPersonAbsences()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var personRequest = createAcceptedRequest(person, dateTimePeriod, false);

			var personAbsence = PersonAbsenceRepository.LoadAll().FirstOrDefault();
			ScheduleStorage.Remove(personAbsence);

			var result = Target.CancelRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, string.Empty);

			result.Success.Should().Be.False();
			result.AffectedRequestIds.Should().Be.Empty();
			personRequest.IsCancelled.Should().Be.False();
		}

		[Test]
		public void ShouldCancelPersonRequest()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			setupStateHolderProxy();

			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var personRequest = createAcceptedRequest(person, dateTimePeriod, true);

			var result = Target.CancelRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, string.Empty);

			result.Success.Should().Be.True();
			result.AffectedRequestIds.ToList().Contains(personRequest.Id.GetValueOrDefault()).Should().Be.True();
			personRequest.IsCancelled.Should().Be.True();
		}

		[Test]
		public void ShouldCancelPersonRequestWithReplyMessage()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			setupStateHolderProxy();

			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var personRequest = createAcceptedRequest(person, dateTimePeriod, true);

			var result = Target.CancelRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, "test");

			result.Success.Should().Be.True();
			result.AffectedRequestIds.ToList().Contains(personRequest.Id.GetValueOrDefault()).Should().Be.True();
			personRequest.IsCancelled.Should().Be.True();
			personRequest.GetMessage(new NoFormatting()).Should().Contain("test");
		}

		[Test]
		public void ShouldAddPlaceholderForEachDayInPeriod()
		{
			setupStateHolderProxy();
			Target.RunWaitlist(new DateTimePeriod(2016, 12, 24, 12, 2016, 12, 31, 12));

			QueuedAbsenceRequestRepository.LoadAll().Count().Should().Be.EqualTo(8);
			QueuedAbsenceRequestRepository.LoadAll().ForEach(x => x.PersonRequest.Should().Be.EqualTo(Guid.Empty));
		}

		[Test]
		public void ShouldReplyRequest()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			const string originalMessage = "Original message\r\nLine 1\r\nLine 2";
			const string replyMessage = "Reply message\r\nLine A\r\nLine B";

			setupStateHolderProxy();

			var person = PersonFactory.CreatePerson("Ashley", "Andeen");
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var personRequest = createAcceptedRequest(person, dateTimePeriod, true);
			personRequest.TrySetMessage(originalMessage);

			var requestId = personRequest.Id.GetValueOrDefault();
			var result = Target.ReplyRequests(new[] { requestId }, replyMessage);

			result.Success.Should().Be.True();
			result.AffectedRequestIds.ToList().Contains(requestId).Should().Be.True();

			var newMessage = personRequest.GetMessage(new NoFormatting());
			newMessage.Should().Be.EqualTo(originalMessage + "\r\n" + replyMessage);
		}

		[Test]
		public void ShouldDoNothingWhenReplyRequestWithEmptyMessage()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			const string originalMessage = "Original message\r\nLine 1\r\nLine 2";

			setupStateHolderProxy();

			var person = PersonFactory.CreatePerson("Ashley", "Andeen");
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var personRequest = createAcceptedRequest(person, dateTimePeriod, true);
			personRequest.TrySetMessage(originalMessage);

			var requestId = personRequest.Id.GetValueOrDefault();
			var result = Target.ReplyRequests(new[] { requestId }, string.Empty);

			result.Success.Should().Be.False();//didn't reply, so false
			result.AffectedRequestIds.Count().Should().Be.EqualTo(0);
			personRequest.GetMessage(new NoFormatting()).Should().Be.EqualTo(originalMessage);
		}

		[Test]
		public void ShouldCancelPersonRequestWithOptimisticLockExceptionAndRetry()
		{
			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			var workflowControlSet = WorkflowControlSetFactory.CreateWorkFlowControlSet(_absence, new PendingAbsenceRequest(),
				false);
			workflowControlSet.SchedulePublishedToDate = new DateTime(2018, 10, 15);
			workflowControlSet.PreferenceInputPeriod = new DateOnlyPeriod(new DateOnly(2016, 1, 1), new DateOnly(2017, 10, 15));
			workflowControlSet.PreferencePeriod = workflowControlSet.PreferenceInputPeriod;
			person.WorkflowControlSet = workflowControlSet;

			var absenceDateTimePeriod = new DateTimePeriod(2018, 04, 15, 00, 2018, 04, 19, 23);
			createShiftsForPeriod(absenceDateTimePeriod, person);

			var accountDay1 = createAccountDay(new DateOnly(2017, 7, 1), TimeSpan.FromDays(0), TimeSpan.FromDays(5),
				TimeSpan.FromDays(0));
			createPersonAbsenceAccount(person, _absence, accountDay1);

			var personRequest = createAcceptedRequest(person, absenceDateTimePeriod, false);
			PersonAbsenceRepository.LoadAll().FirstOrDefault().WithId();

			PersonAbsenceAccountRepository.SetThrowOptimisticLockException(true);
			Target.CancelRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, "test");

			Assert.AreEqual(5, accountDay1.Remaining.TotalDays);
		}

		[Test]
		public void ShouldApproveShiftTradeRequestWithoutCopyFullDayAbsenceToDestinationAgent()
		{
			initializeState();

			var date = new DateOnly(2008, 7, 16);
			var personFrom = PersonFactory.CreatePerson("person from").WithId();

			var personTo = PersonFactory.CreatePerson("person to").WithId();
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(personFrom, personTo, date,
				date).WithId();
			var personFromActivity = new Activity("person from activity").WithId();
			var personToActivity = new Activity("person to activity").WithId();
			var shiftCategory = new ShiftCategory("day");

			var personFromAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personFrom, Scenario.Current(),
				personFromActivity
				, new DateTimePeriod(2008, 7, 16, 8, 2008, 7, 16, 17), shiftCategory).WithId();
			var personFromFullDayAbsence =
				PersonAbsenceFactory.CreatePersonAbsence(personFrom, Scenario.Current()
				, new DateTimePeriod(2008, 7, 16, 8, 2008, 7, 16, 17)).WithId();
			var personToAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personTo, Scenario.Current(),
				personToActivity, new DateTimePeriod(2008, 7, 16, 9, 2008, 7, 16, 18), shiftCategory).WithId();

			ScheduleStorage.Add(personFromAssignment);
			ScheduleStorage.Add(personFromFullDayAbsence);
			ScheduleStorage.Add(personToAssignment);

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] {personFrom, personTo},
				new ScheduleDictionaryLoadOptions(false, false), date.ToDateOnlyPeriod(), Scenario.Current());

			shiftTradeSwapDetail.SchedulePartFrom = scheduleDictionary[personFrom].ScheduledDay(date);
			shiftTradeSwapDetail.SchedulePartTo = scheduleDictionary[personTo].ScheduledDay(date);

			shiftTradeSwapDetail.ChecksumFrom = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartFrom).CalculateChecksum();
			shiftTradeSwapDetail.ChecksumTo = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartTo).CalculateChecksum();

			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
			{
				shiftTradeSwapDetail
			});
			var personRequest = new PersonRequest(personFrom, shiftTradeRequest).WithId();
			personRequest.ForcePending();
			PersonRequestRepository.Add(personRequest);

			Target.ApproveRequests(new[] {personRequest.Id.GetValueOrDefault()}, string.Empty);

			personRequest.IsApproved.Should().Be(true);

			scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personFrom, personTo },
				new ScheduleDictionaryLoadOptions(false, false), date.ToDateOnlyPeriod(), Scenario.Current());

			var scheduleDayFrom = scheduleDictionary[personFrom].ScheduledDay(date);
			var scheduleDayTo = scheduleDictionary[personTo].ScheduledDay(date);

			scheduleDayFrom.PersonAbsenceCollection().Length.Should().Be(1);
			scheduleDayFrom.PersonAssignment().ShiftLayers.FirstOrDefault()?.Payload.Should().Be(personToActivity);

			scheduleDayTo.PersonAbsenceCollection().Length.Should().Be(0);
			scheduleDayTo.PersonAssignment().ShiftLayers.FirstOrDefault()?.Payload.Should().Be(personFromActivity);
		}

		[Test]
		public void ShouldApproveShiftTradeRequestWithoutCopyFullDayAbsenceToSourceAgent()
		{
			initializeState();

			var date = new DateOnly(2008, 7, 16);
			var personFrom = PersonFactory.CreatePerson("person from").WithId();

			var personTo = PersonFactory.CreatePerson("person to").WithId();
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(personFrom, personTo, date,
				date).WithId();
			var personFromActivity = new Activity("person from activity").WithId();
			var personToActivity = new Activity("person to activity").WithId();
			var shiftCategory = new ShiftCategory("day");

			var personFromAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personFrom, Scenario.Current(),
				personFromActivity
				, new DateTimePeriod(2008, 7, 16, 8, 2008, 7, 16, 17), shiftCategory).WithId();
			var personToFullDayAbsence =
				PersonAbsenceFactory.CreatePersonAbsence(personTo, Scenario.Current()
				, new DateTimePeriod(2008, 7, 16, 9, 2008, 7, 16, 18)).WithId();
			var personToAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personTo, Scenario.Current(),
				personToActivity, new DateTimePeriod(2008, 7, 16, 9, 2008, 7, 16, 18), shiftCategory).WithId();

			ScheduleStorage.Add(personFromAssignment);
			ScheduleStorage.Add(personToFullDayAbsence);
			ScheduleStorage.Add(personToAssignment);

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personFrom, personTo },
				new ScheduleDictionaryLoadOptions(false, false), date.ToDateOnlyPeriod(), Scenario.Current());

			shiftTradeSwapDetail.SchedulePartFrom = scheduleDictionary[personFrom].ScheduledDay(date);
			shiftTradeSwapDetail.SchedulePartTo = scheduleDictionary[personTo].ScheduledDay(date);

			shiftTradeSwapDetail.ChecksumFrom = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartFrom).CalculateChecksum();
			shiftTradeSwapDetail.ChecksumTo = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartTo).CalculateChecksum();

			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
			{
				shiftTradeSwapDetail
			});
			var personRequest = new PersonRequest(personFrom, shiftTradeRequest).WithId();
			personRequest.ForcePending();
			PersonRequestRepository.Add(personRequest);

			Target.ApproveRequests(new[] { personRequest.Id.GetValueOrDefault() }, string.Empty);

			personRequest.IsApproved.Should().Be(true);

			scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personFrom, personTo },
				new ScheduleDictionaryLoadOptions(false, false), date.ToDateOnlyPeriod(), Scenario.Current());

			var scheduleDayFrom = scheduleDictionary[personFrom].ScheduledDay(date);
			var scheduleDayTo = scheduleDictionary[personTo].ScheduledDay(date);

			scheduleDayFrom.PersonAbsenceCollection().Length.Should().Be(0);
			scheduleDayFrom.PersonAssignment().ShiftLayers.FirstOrDefault()?.Payload.Should().Be(personToActivity);

			scheduleDayTo.PersonAbsenceCollection().Length.Should().Be(1);
			scheduleDayTo.PersonAssignment().ShiftLayers.FirstOrDefault()?.Payload.Should().Be(personFromActivity);
		}

		[Test]
		public void ShouldApproveShiftTradeRequestWithPartialAndFullDayAbsenceStayInSourceAgent()
		{
			initializeState();

			var date = new DateOnly(2008, 7, 16);
			var personFrom = PersonFactory.CreatePerson("person from").WithId();

			var personTo = PersonFactory.CreatePerson("person to").WithId();
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(personFrom, personTo, date,
				date).WithId();
			var personFromActivity = new Activity("person from activity").WithId();
			var personToActivity = new Activity("person to activity").WithId();
			var shiftCategory = new ShiftCategory("day");

			var personFromAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personFrom, Scenario.Current(),
				personFromActivity
				, new DateTimePeriod(2008, 7, 16, 8, 2008, 7, 16, 17), shiftCategory).WithId();
			var personFromFullDayAbsence =
				PersonAbsenceFactory.CreatePersonAbsence(personFrom, Scenario.Current()
				, new DateTimePeriod(2008, 7, 16, 8, 2008, 7, 16, 17)).WithId();
			var personFromPartialAbsence =
				PersonAbsenceFactory.CreatePersonAbsence(personFrom, Scenario.Current()
					, new DateTimePeriod(2008, 7, 16, 8, 2008, 7, 16, 9)).WithId();
			var personToAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(personTo, Scenario.Current(),
				personToActivity, new DateTimePeriod(2008, 7, 16, 9, 2008, 7, 16, 18), shiftCategory).WithId();

			ScheduleStorage.Add(personFromAssignment);
			ScheduleStorage.Add(personFromPartialAbsence);
			ScheduleStorage.Add(personFromFullDayAbsence);
			ScheduleStorage.Add(personToAssignment);

			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personFrom, personTo },
				new ScheduleDictionaryLoadOptions(false, false), date.ToDateOnlyPeriod(), Scenario.Current());

			shiftTradeSwapDetail.SchedulePartFrom = scheduleDictionary[personFrom].ScheduledDay(date);
			shiftTradeSwapDetail.SchedulePartTo = scheduleDictionary[personTo].ScheduledDay(date);

			shiftTradeSwapDetail.ChecksumFrom = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartFrom).CalculateChecksum();
			shiftTradeSwapDetail.ChecksumTo = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartTo).CalculateChecksum();

			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
			{
				shiftTradeSwapDetail
			});
			var personRequest = new PersonRequest(personFrom, shiftTradeRequest).WithId();
			personRequest.ForcePending();
			PersonRequestRepository.Add(personRequest);

			Target.ApproveRequests(new[] { personRequest.Id.GetValueOrDefault() }, string.Empty);

			personRequest.IsApproved.Should().Be(true);

			scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personFrom, personTo },
				new ScheduleDictionaryLoadOptions(false, false), date.ToDateOnlyPeriod(), Scenario.Current());

			var scheduleDayFrom = scheduleDictionary[personFrom].ScheduledDay(date);
			var scheduleDayTo = scheduleDictionary[personTo].ScheduledDay(date);

			scheduleDayFrom.PersonAbsenceCollection().Length.Should().Be(2);
			scheduleDayFrom.PersonAssignment().ShiftLayers.FirstOrDefault()?.Payload.Should().Be(personToActivity);

			scheduleDayTo.PersonAbsenceCollection().Length.Should().Be(0);
			scheduleDayTo.PersonAssignment().ShiftLayers.FirstOrDefault()?.Payload.Should().Be(personFromActivity);
		}

		[Test]
		[SetCulture("en-US")]
		public void ShouldNotBeAbleToApproveApprovedRequest()
		{
			var person = PersonFactory.CreatePersonWithId();
			var absence = AbsenceFactory.CreateAbsenceWithId();

			var absenceDateTimePeriod = new DateTimePeriod(2016, 01, 01, 00, 2016, 01, 01, 23);

			var personRequest = createAbsenceRequest(person, absence, absenceDateTimePeriod, false);
			personRequest.ForcePending();
			var personRequestCheckAuthorization = new PersonRequestAuthorizationCheckerConfigurable();
			personRequest.Approve(new ApprovalServiceForTest(), personRequestCheckAuthorization);

			var result = Target.ApproveRequests(new[] {personRequest.Id.GetValueOrDefault()}, string.Empty);

			Assert.IsFalse(result.Success);
			Assert.IsTrue(result.ErrorMessages.Contains("A request that is Approved cannot be Approved."),
				string.Join(",", result.ErrorMessages));
			Assert.IsTrue(personRequest.IsApproved);
		}

		[TearDown]
		public void AfterTest()
		{
			StateHolderProxyHelper.ClearStateHolder();
			Thread.CurrentPrincipal = null;
		}

		private void createShiftsForPeriod(DateTimePeriod period, IPerson person)
		{
			foreach (var day in period.WholeDayCollection(TimeZoneInfo.Utc))
			{
				ScheduleStorage.Add(createAssignment(person, day.StartDateTime, day.EndDateTime, Scenario.Current()));
			}
		}

		private static IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, IScenario scenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(person,
				scenario, new DateTimePeriod(startDate, endDate)).WithId();
		}

		private AccountDay createAccountDay(DateOnly startDate, TimeSpan balanceIn, TimeSpan accrued, TimeSpan balance)
		{
			return new AccountDay(startDate)
			{
				BalanceIn = balanceIn,
				Accrued = accrued,
				Extra = TimeSpan.FromDays(0),
				LatestCalculatedBalance = balance
			};
		}

		private void createPersonAbsenceAccount(IPerson person, IAbsence absence, params IAccount[] accountDays)
		{
			var personAbsenceAccount = PersonAbsenceAccountFactory.CreatePersonAbsenceAccount(person, absence,
				accountDays);
			PersonAbsenceAccountRepository.Add(personAbsenceAccount);
		}

		private static void setupStateHolderProxy()
		{
			var stateMock = new FakeState();
			var dataSource = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);
			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
		}

		private IPersonRequest createAcceptedRequest(IPerson person, DateTimePeriod dateTimePeriod,
			bool associatePersonAbsence)
		{
			var personRequest = createNewAbsenceRequest(person, _absence, dateTimePeriod);

			personRequest.Pending();

			Target.ApproveRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, string.Empty);

			if (!associatePersonAbsence) return personRequest;

			return personRequest;
		}

		private string getWriteProtectMessage(IPerson person, DateTime date)
		{
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var culture = UserCulture.GetCulture();

			return string.Format(Resources.ScheduleIsWriteProtected,
				CommonAgentNameProvider.CommonAgentNameSettings.BuildFor(person),
				TimeZoneHelper.ConvertFromUtc(date, timeZone).ToString(culture.DateTimeFormat.ShortDatePattern, culture));
		}

		private RequestCommandHandlingResult doApproveAbsenceWriteProtectedTest(IPerson person, DateTimePeriod dateTimePeriod)
		{
			var absence = AbsenceFactory.CreateAbsence("absence");
			var personRequest = createNewAbsenceRequest(person, absence, dateTimePeriod);

			personRequest.Pending();

			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(dateTimePeriod.StartDateTime);

			var result = Target.ApproveRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, string.Empty);
			return result;
		}

		private RequestCommandHandlingResult doCancelAbsenceWriteProtectedTest(IPerson person, DateTimePeriod dateTimePeriod)
		{
			var absence = AbsenceFactory.CreateAbsence("absence");

			var personRequest = createNewAbsenceRequest(person, absence, dateTimePeriod);

			var personAbsence = new PersonAbsence(person, Scenario.Current(),
				new AbsenceLayer(absence, dateTimePeriod));
			PersonAbsenceRepository.Add(personAbsence);

			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(dateTimePeriod.StartDateTime);

			var result = Target.CancelRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, string.Empty);
			return result;
		}

		private IPersonRequest createWaitlistedAbsenceRequest(IPerson person, IAbsence absence,
			DateTimePeriod requestDateTimePeriod)
		{
			return createAbsenceRequest(person, absence, requestDateTimePeriod, true);
		}

		private IPersonRequest createNewAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			return createAbsenceRequest(person, absence, requestDateTimePeriod, false);
		}

		private IPersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod,
			bool isAutoDenied)
		{
			var absenceRequest = new AbsenceRequest(absence, requestDateTimePeriod).WithId();
			var personRequest = new PersonRequest(person, absenceRequest).WithId();

			if (isAutoDenied)
			{
				personRequest.Deny("Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			}

			PersonRequestRepository.Add(personRequest);

			return personRequest;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence,
			bool isWaitListEnabled)
		{
			var workflowControlSet = new WorkflowControlSet
			{
				AbsenceRequestWaitlistEnabled = isWaitListEnabled
			};

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod,
				AbsenceRequestProcess = new GrantAbsenceRequest()
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;
		}

		private static void initializeState()
		{
			var dataSource = new DataSource(UnitOfWorkFactoryFactoryForTest.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			loggedOnPerson.PermissionInformation.SetDefaultTimeZone(
				TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			var principal = new TeleoptiPrincipalFactory().MakePrincipal(loggedOnPerson, dataSource,
				BusinessUnitFactory.BusinessUnitUsedInTest, null);
			Thread.CurrentPrincipal = principal;

			StateHolderProxyHelper.ClearStateHolder();
			StateHolder.Initialize(new FakeState(), new MessageBrokerCompositeDummy());
		}

		public class FakePersonAbsenceAccountRepositoryWithOptimisticLockException : FakePersonAbsenceAccountRepository, IPersonAbsenceAccountRepository
		{
			public bool ThrowOptimisticLockException { get; private set; }

			public void SetThrowOptimisticLockException(bool throwOptimisticLockException)
			{
				ThrowOptimisticLockException = throwOptimisticLockException;
			}

			public new void Add(IPersonAbsenceAccount entity)
			{
				if (ThrowOptimisticLockException)
				{
					ThrowOptimisticLockException = false;
					throw new OptimisticLockException();
				}
				base.Add(entity);
			}
		}
	}

#pragma warning restore 0649
}