using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
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
		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;
		public Global.FakePermissionProvider PermissionProvider;
		public ConfigurablePermissions Authorization;
		public IPersonRequestCheckAuthorization PersonRequestCheckAuthorization;
		public ICommonAgentNameProvider CommonAgentNameProvider;
		public ILoggedOnUser LoggedOnUser;
		public IUserCulture UserCulture;
		
		[Test]
		public void TargetShouldNotBeNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotHandleApproveCommandWithInvalidRequestId()
		{
			var result = Target.ApproveRequests(new List<Guid> { new Guid() });
			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldInvokeApproveAbsenceFromRequestApprovalServiceWithValidAbsenceRequest()
		{
			var person = PersonFactory.CreatePerson("tester");
			var scheduleDictionary = new FakeScheduleDictionary();

			var requestApprovalService = RequestApprovalServiceFactory.MakeRequestApprovalServiceScheduler(scheduleDictionary,
				Scenario.Current(), person);

			var absence = AbsenceFactory.CreateAbsence("absence");
			var personRequest = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var absenceRequest = personRequest.Request as AbsenceRequest;

			requestApprovalService.Stub(x => x.ApproveAbsence(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9), person, absenceRequest)).Return(new List<IBusinessRuleResponse>());
			Target.ApproveRequests(new List<Guid> { personRequest.Id.Value });
			requestApprovalService.AssertWasCalled(x => x.ApproveAbsence(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9), person, absenceRequest), options => options.Repeat.AtLeastOnce());
		}

		[Test]
		public void ShouldApproveAbsenceRequestWithPendingStatus()
		{
			var person = PersonFactory.CreatePerson("tester");
			var scheduleDictionary = new FakeScheduleDictionary();

			var requestApprovalService = RequestApprovalServiceFactory.MakeRequestApprovalServiceScheduler(scheduleDictionary,
				Scenario.Current(), person);

			var absence = AbsenceFactory.CreateAbsence("absence");
			var personRequest = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var absenceRequest = personRequest.Request as AbsenceRequest;
			personRequest.Pending();

			requestApprovalService.Stub(x => x.ApproveAbsence(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9), person, absenceRequest)).Return(new List<IBusinessRuleResponse>());
			var result = Target.ApproveRequests(new List<Guid> { personRequest.Id.Value });

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldApproveAllAbsenceRequestsWithPendingStatus()
		{
			var person = PersonFactory.CreatePerson("tester");
			var scheduleDictionary = new FakeScheduleDictionary();

			var requestApprovalService = RequestApprovalServiceFactory.MakeRequestApprovalServiceScheduler(scheduleDictionary,
				Scenario.Current(), person);

			var absence = AbsenceFactory.CreateAbsence("absence");

			var personRequest1 = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var personRequest2 = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));

			personRequest1.Pending();
			personRequest2.Pending();

			var absenceRequest1 = personRequest1.Request as AbsenceRequest;
			var absenceRequest2 = personRequest2.Request as AbsenceRequest;

			requestApprovalService.Stub(x => x.ApproveAbsence(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9), person, absenceRequest1)).Return(new List<IBusinessRuleResponse>());
			requestApprovalService.Stub(x => x.ApproveAbsence(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9), person, absenceRequest2)).Return(new List<IBusinessRuleResponse>());

			var result = Target.ApproveRequests(new List<Guid> { personRequest1.Id.Value, personRequest2.Id.Value });

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldNotHandleDenyCommandWithInvalidRequestId()
		{
			var result = Target.DenyRequests(new List<Guid> { new Guid() });
			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldDenyAbsenceRequestWithPendingStatus()
		{

			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");

			var personRequest = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			personRequest.Pending();

			var result = Target.DenyRequests(new List<Guid> { personRequest.Id.Value });

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldDenyAllAbsenceRequestsWithPendingStatus()
		{


			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");
			var personRequest1 = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var personRequest2 = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			personRequest1.Pending();
			personRequest2.Pending();

			var result = Target.DenyRequests(new List<Guid> { personRequest1.Id.Value, personRequest2.Id.Value });

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldManuallyDenyWaitlistRequest()
		{
			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");

			person.WorkflowControlSet = createWorkFlowControlSet(new DateTime(2016, 2, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2016, 4, 1, 23, 00, 00, DateTimeKind.Utc), absence, true);
			var waitlistedPersonRequest = createWaitlistedAbsenceRequest(person, absence,
				new DateTimePeriod(
					new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc)));

			Target.DenyRequests(new List<Guid> { waitlistedPersonRequest.Id.Value });
			waitlistedPersonRequest.IsWaitlisted.Should().Be.False();
			waitlistedPersonRequest.IsDenied.Should().Be.True();
		}


		[Test]
		public void ShouldManuallyApproveWaitlistedRequests()
		{
			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");
			var scheduleDictionary = new FakeScheduleDictionary();

			person.WorkflowControlSet = createWorkFlowControlSet(new DateTime(2016, 2, 1, 10, 0, 0, DateTimeKind.Utc), DateTime.Today, absence, true);
			var requestApprovalService = RequestApprovalServiceFactory.MakeRequestApprovalServiceScheduler(scheduleDictionary, Scenario.Current(), person);
			
			var dateTimePeriod = new DateTimePeriod(
				new DateTime(2016, 3, 1, 10, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 3, 1, 23, 00, 00, DateTimeKind.Utc));
			
			var waitlistedPersonRequest = createWaitlistedAbsenceRequest(person, absence, dateTimePeriod);
			
			requestApprovalService.Stub(x => x.ApproveAbsence(absence, dateTimePeriod, person)).IgnoreArguments().Return(new List<IBusinessRuleResponse>());
			Target.ApproveRequests(new List<Guid> { waitlistedPersonRequest.Id.Value });

			waitlistedPersonRequest.IsWaitlisted.Should().Be.False();
			waitlistedPersonRequest.IsDenied.Should().Be.False();
			waitlistedPersonRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldReturnWriteProtectedMsgWhenAttemptingToApproveAnAbsenceWhereScheduleIsWriteProtected()
		{
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
			var person = PersonFactory.CreatePerson("Yngwie","Malmsteen");
			var dateTimePeriod =  new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var writeProtectErrorMessage = getWriteProtectMessage (person, dateTimePeriod.StartDateTime);

			Authorization.HasPermission(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule);
			var result = doApproveAbsenceWriteProtectedTest(person, dateTimePeriod);
			result.ErrorMessages.Contains(writeProtectErrorMessage).Should().Be.False();
		}

		[Test]
		public void ShouldReturnWriteProtectedMsgWhenAttemptingToCancelAnAbsenceWhereScheduleIsWriteProtected()
		{
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
			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var writeProtectErrorMessage = getWriteProtectMessage(person, dateTimePeriod.StartDateTime);

			Authorization.HasPermission(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule);
			((PersonRequestAuthorizationCheckerConfigurable) PersonRequestCheckAuthorization).HasCancelPermission = false;

			var result = doCancelAbsenceWriteProtectedTest(person, dateTimePeriod);
			result.ErrorMessages.Contains(writeProtectErrorMessage).Should().Be.False();
		}

		[Test]
		public void ShouldNotCancelPersonRequestWhenNoPersonAbsences()
		{
			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var personRequest = createAcceptedRequest(person, dateTimePeriod, false);

			var result = Target.CancelRequests (new List<Guid> {personRequest.Id.Value});

			result.Success.Should().Be.False();
			result.AffectedRequestIds.Should().Be.Empty();
			personRequest.IsCancelled.Should().Be.False();
			
		}

		[Test]
		public void ShouldCancelPersonRequest()
		{
			setupStateHolderProxy();

			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen");
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 9);
			var personRequest = createAcceptedRequest(person, dateTimePeriod, true);
			
			var result = Target.CancelRequests(new List<Guid> { personRequest.Id.Value });

			result.Success.Should().Be.True();
			result.AffectedRequestIds.ToList().Contains (personRequest.Id.Value).Should().Be.True();
			personRequest.IsCancelled.Should().Be.True();
		}

		[Test]
		public void ShouldRunRequestWaitlist()
		{
			var period = new DateTimePeriod(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
			var result = Target.RunWaitlist(period);

			result.Success.Should().Be.True();
			result.AffectedRequestIds.Any().Should().Be.False();
		}

		private static void setupStateHolderProxy()
		{
			var stateMock = new FakeState();
			var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);
			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
		}


		private IPersonRequest createAcceptedRequest (IPerson person, DateTimePeriod dateTimePeriod, bool associatePersonAbsence)
		{
			var scheduleDictionary = new FakeScheduleDictionary();
			var requestApprovalService = RequestApprovalServiceFactory.MakeRequestApprovalServiceScheduler (scheduleDictionary, Scenario.Current(), person);
			
			var absence = AbsenceFactory.CreateAbsence ("absence");
			var personRequest = createNewAbsenceRequest (person, absence, dateTimePeriod);
			var absenceRequest = personRequest.Request as AbsenceRequest;

			personRequest.Pending();

			requestApprovalService.Stub (x => x.ApproveAbsence (absence, dateTimePeriod, person, associatePersonAbsence? absenceRequest : null))
				.IgnoreArguments()
				.Return (new List<IBusinessRuleResponse>());


			Target.ApproveRequests (new List<Guid> {personRequest.Id.Value});

			if (associatePersonAbsence)
			{
				var personAbsence = PersonAbsenceFactory.CreatePersonAbsence (person, Scenario.Current(), dateTimePeriod, absence).WithId();
				((FakePersonAbsenceRepository)PersonAbsenceRepository).Add (personAbsence);
				personAbsence.AbsenceRequest = absenceRequest;
				absenceRequest.PersonAbsences.Add (personAbsence);

				ScheduleStorage.Add (personAbsence);
			}

			return personRequest;
		}


		private string getWriteProtectMessage (IPerson person, DateTime date)
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

			var requestApprovalService = RequestApprovalServiceFactory.MakeRequestApprovalServiceScheduler(scheduleDictionary,
				Scenario.Current(), person);

			var absence = AbsenceFactory.CreateAbsence("absence");
			var personRequest = createNewAbsenceRequest(person, absence, dateTimePeriod);
			var absenceRequest = personRequest.Request as AbsenceRequest;
			personRequest.Pending();

			requestApprovalService.Stub(x => x.ApproveAbsence(absence, dateTimePeriod, person, absenceRequest)).Return(new List<IBusinessRuleResponse>());

			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(dateTimePeriod.StartDateTime); ;

			var result = Target.ApproveRequests (new List<Guid> {personRequest.Id.Value});
			return result;
		}

		private RequestCommandHandlingResult doCancelAbsenceWriteProtectedTest(IPerson person, DateTimePeriod dateTimePeriod)
		{
			var absence = AbsenceFactory.CreateAbsence ("absence");
			
			var personRequest = createNewAbsenceRequest (person, absence, dateTimePeriod );

			var personAbsence = new PersonAbsence (person, Scenario.Current(), new AbsenceLayer (absence, dateTimePeriod), (IAbsenceRequest)personRequest.Request);
			((FakePersonAbsenceRepository)PersonAbsenceRepository).Add(personAbsence);
			
			person.PersonWriteProtection.PersonWriteProtectedDate = new DateOnly(dateTimePeriod.StartDateTime);

			
			var result = Target.CancelRequests (new List<Guid> {personRequest.Id.Value});
			return result;
		}


		private IPersonRequest createWaitlistedAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			return createAbsenceRequest(person, absence, requestDateTimePeriod, true);
		}

		private IPersonRequest createNewAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			return createAbsenceRequest(person, absence, requestDateTimePeriod, false);
		}

		private IPersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod, bool isAutoDenied)
		{
			var absenceRequest = new AbsenceRequest(absence, requestDateTimePeriod).WithId();
			var personRequest = new PersonRequest(person, absenceRequest).WithId();
			
			if (isAutoDenied)
			{

				personRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			}

			PersonRequestRepository.Add(personRequest);

			return personRequest;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence, bool isWaitListEnabled)
		{
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = isWaitListEnabled };

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
