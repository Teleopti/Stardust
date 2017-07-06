using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.Provider
{
#pragma warning disable 0649

	[TestFixture, RequestsTest]
	public class RequestCommandHandlingProviderTest
	{
		public IRequestCommandHandlingProvider Target;
		public ICurrentScenario Scenario;
		public IScheduleStorage ScheduleStorage;
		public IPersonRequestRepository PersonRequestRepository;
		public IPersonAbsenceRepository PersonAbsenceRepository;
		public IQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public Global.FakePermissionProvider PermissionProvider;
		public FakePermissions Authorization;
		public IPersonRequestCheckAuthorization PersonRequestCheckAuthorization;
		public ICommonAgentNameProvider CommonAgentNameProvider;
		public ILoggedOnUser LoggedOnUser;
		public IUserCulture UserCulture;
		public MutableNow Now;

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

		[Ignore("interface has been changed"), Test]
		public void ShouldInvokeApproveAbsenceFromRequestApprovalServiceWithValidAbsenceRequest()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("tester");
			var scheduleDictionary = new FakeScheduleDictionary();

			var absence = AbsenceFactory.CreateAbsence("absence");
			var personRequest = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));

			var requestApprovalService = RequestApprovalServiceFactory.MakeAbsenceRequestApprovalService(
				scheduleDictionary, Scenario.Current(), personRequest);

			requestApprovalService.Stub(
				x => x.Approve(personRequest))
				.Return(new List<IBusinessRuleResponse>());
			Target.ApproveRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, string.Empty);
			requestApprovalService.AssertWasCalled(
				x => x.Approve(personRequest),
				options => options.Repeat.AtLeastOnce());
		}

		[Test]
		public void ShouldApproveAbsenceRequestWithPendingStatus()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("tester");
			var scheduleDictionary = new FakeScheduleDictionary();

			var absence = AbsenceFactory.CreateAbsence("absence");
			var personRequest = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			personRequest.Pending();

			var requestApprovalService = RequestApprovalServiceFactory.MakeAbsenceRequestApprovalService(
				scheduleDictionary, Scenario.Current(), personRequest);

			requestApprovalService.Stub(
				x => x.Approve(personRequest))
				.Return(new List<IBusinessRuleResponse>());
			var result = Target.ApproveRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, string.Empty);

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldApproveAbsenceRequestWithReplyMessage()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("tester");
			var scheduleDictionary = new FakeScheduleDictionary();

			var absence = AbsenceFactory.CreateAbsence("absence");
			var personRequest = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			personRequest.Pending();

			var requestApprovalService = RequestApprovalServiceFactory.MakeAbsenceRequestApprovalService(
				scheduleDictionary, Scenario.Current(), personRequest);

			requestApprovalService.Stub(
				x => x.Approve(personRequest))
				.Return(new List<IBusinessRuleResponse>());
			var result = Target.ApproveRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, "test");

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(1);
			personRequest.IsApproved.Should().Be(true);
			personRequest.GetMessage(new NoFormatting()).Should().Contain("test");
		}

		[Test]
		public void ShouldApproveAllAbsenceRequestsWithPendingStatus()
		{
			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var person = PersonFactory.CreatePerson("tester");
			var scheduleDictionary = new FakeScheduleDictionary();

			var absence = AbsenceFactory.CreateAbsence("absence");

			var personRequest1 = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var personRequest2 = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));

			var requestApprovalService = RequestApprovalServiceFactory.MakeAbsenceRequestApprovalService(
				scheduleDictionary, Scenario.Current(), personRequest1);

			personRequest1.Pending();
			personRequest2.Pending();

			requestApprovalService.Stub(
				x => x.Approve(personRequest1))
				.Return(new List<IBusinessRuleResponse>());
			requestApprovalService.Stub(
				x => x.Approve(personRequest2))
				.Return(new List<IBusinessRuleResponse>());

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
			var cmdDispatcher = MockRepository.GenerateMock<ICommandDispatcher>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(PersonFactory.CreatePersonWithId());

			var target = new RequestCommandHandlingProvider(cmdDispatcher, loggedOnUser);
			var result = target.ApproveWithValidators(requestIds, RequestValidatorsFlag.BudgetAllotmentValidator);
			cmdDispatcher.AssertWasCalled(
				dispatcher => dispatcher.Execute(
					Arg<ApproveBatchRequestsCommand>.Matches(
						cmd => cmd.PersonRequestIdList.Equals(requestIds)
								&& (cmd.Validator == RequestValidatorsFlag.BudgetAllotmentValidator))));

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
			var scheduleDictionary = new FakeScheduleDictionary();

			person.WorkflowControlSet = createWorkFlowControlSet(new DateTime(2016, 2, 1, 10, 0, 0, DateTimeKind.Utc),
				DateTime.Today, absence, true);
			var dateTimePeriod = new DateTimePeriod( new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc));

			var waitlistedPersonRequest = createWaitlistedAbsenceRequest(person, absence, dateTimePeriod);
			var requestApprovalService = RequestApprovalServiceFactory.MakeAbsenceRequestApprovalService(
				scheduleDictionary, Scenario.Current(), waitlistedPersonRequest);

			requestApprovalService.Stub(x => x.Approve(waitlistedPersonRequest))
				.IgnoreArguments()
				.Return(new List<IBusinessRuleResponse>());
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

			Authorization.HasPermission(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule);
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

			Authorization.HasPermission(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule);
			((PersonRequestAuthorizationCheckerConfigurable)PersonRequestCheckAuthorization).HasCancelPermission = false;

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

			QueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(8);
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

		private static void setupStateHolderProxy()
		{
			var stateMock = new FakeState();
			var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);
			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
		}

		private IPersonRequest createAcceptedRequest(IPerson person, DateTimePeriod dateTimePeriod,
			bool associatePersonAbsence)
		{
			var absence = AbsenceFactory.CreateAbsence("absence");
			var personRequest = createNewAbsenceRequest(person, absence, dateTimePeriod);
			var scheduleDictionary = new FakeScheduleDictionary();
			var requestApprovalService = RequestApprovalServiceFactory.MakeAbsenceRequestApprovalService(
				scheduleDictionary, Scenario.Current(), personRequest);

			personRequest.Pending();

			requestApprovalService.Stub(
				x => x.Approve(personRequest))
				.IgnoreArguments()
				.Return(new List<IBusinessRuleResponse>());

			Target.ApproveRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, string.Empty);

			if (!associatePersonAbsence) return personRequest;

			var personAbsence =
				PersonAbsenceFactory.CreatePersonAbsence(person, Scenario.Current(), dateTimePeriod, absence).WithId();
			((FakePersonAbsenceRepository)PersonAbsenceRepository).Add(personAbsence);

			ScheduleStorage.Add(personAbsence);

			return personRequest;
		}

		private string getWriteProtectMessage(IPerson person, DateTime date)
		{
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var culture = UserCulture.GetCulture();

			return string.Format(Resources.ScheduleIsWriteProtected,
				CommonAgentNameProvider.CommonAgentNameSettings.BuildCommonNameDescription(person),
				TimeZoneHelper.ConvertFromUtc(date, timeZone).ToString(culture.DateTimeFormat.ShortDatePattern, culture));
		}

		private RequestCommandHandlingResult doApproveAbsenceWriteProtectedTest(IPerson person, DateTimePeriod dateTimePeriod)
		{
			var scheduleDictionary = new FakeScheduleDictionary();
			var absence = AbsenceFactory.CreateAbsence("absence");
			var personRequest = createNewAbsenceRequest(person, absence, dateTimePeriod);
			var requestApprovalService = RequestApprovalServiceFactory.MakeAbsenceRequestApprovalService(
				scheduleDictionary, Scenario.Current(), personRequest);

			personRequest.Pending();

			requestApprovalService.Stub(x => x.Approve(personRequest))
				.Return(new List<IBusinessRuleResponse>());

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
			((FakePersonAbsenceRepository)PersonAbsenceRepository).Add(personAbsence);

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
	}

#pragma warning restore 0649
}