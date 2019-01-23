using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;


namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.Provider
{
#pragma warning disable 0649

	[TestFixture, DomainTest]
	public class RequestCommandHandlingProviderTest : IIsolateSystem
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
		public FullPermission Permission;
		private IAbsence _absence;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PersonRequestAuthorizationCheckerConfigurable>().For<IPersonRequestCheckAuthorization>();
			isolate.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario {DefaultScenario = true})).For<IScenarioRepository>();
			isolate.UseTestDouble<RequestCommandHandlingProvider>().For<IRequestCommandHandlingProvider>();
			isolate.UseTestDouble<SignificantPartService>().For<ISignificantPartService>();
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
		public void ShouldCancelPersonRequestsAndRemoveRelatedSchedulesForOneDay()
		{
			Now.Is(new DateTime(2015, 10, 3, 22, 0, 0, DateTimeKind.Utc));

			setupStateHolderProxy();
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 4);
			var date = new DateOnly(2015, 10, 3);

			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen").WithId();
			var personActivity = new Activity("person activity1").WithId();
			var shiftCategory = new ShiftCategory("day");
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, Scenario.Current(),
				personActivity
				, dateTimePeriod, shiftCategory).WithId();
			ScheduleStorage.Add(personAssignment);

			var personRequest = createAcceptedRequest(person, dateTimePeriod, true);
			
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { person },
				new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(date, date), Scenario.Current());

			Assert.IsTrue(scheduleDictionary[person].ScheduledDay(date).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);

			var personRequests = new List<Guid> { personRequest.Id.GetValueOrDefault() };
			var result = Target.CancelRequests(personRequests, string.Empty);

			scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { person },
				new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(date, date), Scenario.Current());

			result.Success.Should().Be.True();
			result.AffectedRequestIds.ToList().Contains(personRequest.Id.GetValueOrDefault()).Should().Be.True();
			personRequest.IsCancelled.Should().Be.True();

			var scheduleDay = scheduleDictionary[person].ScheduledDay(date);
			Assert.IsFalse(scheduleDay.SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);
		}

		[Test]
		public void ShouldCancelPersonRequestsAndRemoveRelatedSchedulesForMultiDays()
		{
			Now.Is(new DateTime(2015, 10, 3, 22, 0, 0, DateTimeKind.Utc));

			setupStateHolderProxy();
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 4);

			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen").WithId();
			var shiftCategory = new ShiftCategory("day");

			var personActivity1 = new Activity("person activity1").WithId();
			var personAssignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, Scenario.Current(),
				personActivity1
				, dateTimePeriod, shiftCategory).WithId();
			ScheduleStorage.Add(personAssignment1);

			var personActivity2 = new Activity("person activity2").WithId();
			var personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, Scenario.Current(),
				personActivity2
				, dateTimePeriod.ChangeEndTime(TimeSpan.FromDays(1)).ChangeStartTime(TimeSpan.FromDays(1)), shiftCategory).WithId();
			ScheduleStorage.Add(personAssignment2);

			var personActivity3 = new Activity("person activity3").WithId();
			var personAssignment3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, Scenario.Current(),
				personActivity3
				, dateTimePeriod.ChangeEndTime(TimeSpan.FromDays(2)).ChangeStartTime(TimeSpan.FromDays(2)), shiftCategory).WithId();
			ScheduleStorage.Add(personAssignment3);

			var personRequest1 = createAcceptedRequest(person, dateTimePeriod, true);
			var personRequest2 = createAcceptedRequest(person, dateTimePeriod.ChangeEndTime(TimeSpan.FromDays(1)).ChangeStartTime(TimeSpan.FromDays(1)), true);
			var personRequest3 = createAcceptedRequest(person, dateTimePeriod.ChangeEndTime(TimeSpan.FromDays(2)).ChangeStartTime(TimeSpan.FromDays(2)), true);

			var firstDay = new DateOnly(2015, 10, 3);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { person },
				new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(firstDay, firstDay.AddDays(2)), Scenario.Current());

			Assert.IsTrue(scheduleDictionary[person].ScheduledDay(firstDay).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);
			Assert.IsTrue(scheduleDictionary[person].ScheduledDay(firstDay.AddDays(1)).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);
			Assert.IsTrue(scheduleDictionary[person].ScheduledDay(firstDay.AddDays(2)).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);
			
			var personRequests = new List<Guid> { personRequest1.Id.GetValueOrDefault(), personRequest2.Id.GetValueOrDefault(), personRequest3.Id.GetValueOrDefault() };
			var result = Target.CancelRequests(personRequests, string.Empty);

			scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { person },
				new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(firstDay, firstDay.AddDays(2)), Scenario.Current());

			result.Success.Should().Be.True();
			result.AffectedRequestIds.ToList().Contains(personRequest1.Id.GetValueOrDefault()).Should().Be.True();
			personRequest1.IsCancelled.Should().Be.True();
			personRequest2.IsCancelled.Should().Be.True();
			personRequest3.IsCancelled.Should().Be.True();

			Assert.IsFalse(scheduleDictionary[person].ScheduledDay(firstDay).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);
			Assert.IsFalse(scheduleDictionary[person].ScheduledDay(firstDay.AddDays(1)).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);
			Assert.IsFalse(scheduleDictionary[person].ScheduledDay(firstDay.AddDays(2)).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);

			scheduleDictionary[person].ScheduledDay(firstDay).PersonAssignment().ShiftLayers.FirstOrDefault()?.Payload.Should().Be(personActivity1);
			scheduleDictionary[person].ScheduledDay(firstDay.AddDays(1)).PersonAssignment().ShiftLayers.FirstOrDefault()?.Payload.Should().Be(personActivity2);
			scheduleDictionary[person].ScheduledDay(firstDay.AddDays(2)).PersonAssignment().ShiftLayers.FirstOrDefault()?.Payload.Should().Be(personActivity3);
		}

		[Test]
		public void ShouldCancelPersonRequestsAndRemoveRelatedSchedulesInCentralTimeZone()
		{
			Now.Is(new DateTime(2015, 10, 1, 22, 0, 0, DateTimeKind.Utc));

			setupStateHolderProxy();
			var dateTimePeriod = new DateTimePeriod(2015, 10, 3, 2015, 10, 4);

			var person = PersonFactory.CreatePerson("Yngwie", "Malmsteen").WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfoFactory.CentralStandardTime());
			var shiftCategory = new ShiftCategory("day");

			var personActivity1 = new Activity("person activity1").WithId();
			var personAssignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, Scenario.Current(),
				personActivity1
				, dateTimePeriod, shiftCategory).WithId();
			ScheduleStorage.Add(personAssignment1);

			var personActivity2 = new Activity("person activity2").WithId();
			var personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, Scenario.Current(),
				personActivity2
				, dateTimePeriod.ChangeEndTime(TimeSpan.FromDays(1)).ChangeStartTime(TimeSpan.FromDays(1)), shiftCategory).WithId();
			ScheduleStorage.Add(personAssignment2);

			var personActivity3 = new Activity("person activity3").WithId();
			var personAssignment3 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, Scenario.Current(),
				personActivity3
				, dateTimePeriod.ChangeEndTime(TimeSpan.FromDays(2)).ChangeStartTime(TimeSpan.FromDays(2)), shiftCategory).WithId();
			ScheduleStorage.Add(personAssignment3);

			var personRequest1 = createAcceptedRequest(person, dateTimePeriod, true);
			var personRequest2 = createAcceptedRequest(person, dateTimePeriod.ChangeEndTime(TimeSpan.FromDays(1)).ChangeStartTime(TimeSpan.FromDays(1)), true);
			var personRequest3 = createAcceptedRequest(person, dateTimePeriod.ChangeEndTime(TimeSpan.FromDays(2)).ChangeStartTime(TimeSpan.FromDays(2)), true);

			var firstDay = new DateOnly(2015, 10, 2);
			var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { person },
				new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(firstDay, firstDay.AddDays(2)), Scenario.Current());

			Assert.IsTrue(scheduleDictionary[person].ScheduledDay(firstDay).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);
			Assert.IsTrue(scheduleDictionary[person].ScheduledDay(firstDay.AddDays(1)).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);
			Assert.IsTrue(scheduleDictionary[person].ScheduledDay(firstDay.AddDays(2)).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);

			var personRequests = new List<Guid> { personRequest1.Id.GetValueOrDefault(), personRequest2.Id.GetValueOrDefault(), personRequest3.Id.GetValueOrDefault() };
			var result = Target.CancelRequests(personRequests, string.Empty);

			scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { person },
				new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(firstDay, firstDay.AddDays(2)), Scenario.Current());

			result.Success.Should().Be.True();
			result.AffectedRequestIds.ToList().Contains(personRequest1.Id.GetValueOrDefault()).Should().Be.True();
			personRequest1.IsCancelled.Should().Be.True();
			personRequest2.IsCancelled.Should().Be.True();
			personRequest3.IsCancelled.Should().Be.True();

			Assert.IsFalse(scheduleDictionary[person].ScheduledDay(firstDay).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);
			Assert.IsFalse(scheduleDictionary[person].ScheduledDay(firstDay.AddDays(1)).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);
			Assert.IsFalse(scheduleDictionary[person].ScheduledDay(firstDay.AddDays(2)).SignificantPartForDisplay() == SchedulePartView.FullDayAbsence);

			scheduleDictionary[person].ScheduledDay(firstDay).PersonAssignment().ShiftLayers.FirstOrDefault()?.Payload.Should().Be(personActivity1);
			scheduleDictionary[person].ScheduledDay(firstDay.AddDays(1)).PersonAssignment().ShiftLayers.FirstOrDefault()?.Payload.Should().Be(personActivity2);
			scheduleDictionary[person].ScheduledDay(firstDay.AddDays(2)).PersonAssignment().ShiftLayers.FirstOrDefault()?.Payload.Should().Be(personActivity3);
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

		[Test]
		public void ShouldNotDenyWhenTeamLeaderHasNoPermissionToEditRestrictedScenarios()
		{
			Scenario.Current().Restricted = true;
			Permission.AddToBlackList(DefinedRaptorApplicationFunctionPaths.ModifyRestrictedScenario);

			Now.Is(new DateTime(2016, 12, 22, 22, 0, 0, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");

			var personRequest = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			personRequest.Pending();

			var result = Target.DenyRequests(new List<Guid> { personRequest.Id.GetValueOrDefault() }, string.Empty);

			result.AffectedRequestIds.ToList().Count.Should().Be.EqualTo(0);
			result.ErrorMessages.First().Should().Be
				.EqualTo(Resources.CanNotApproveOrDenyRequestDueToNoPermissionToModifyRestrictedScenarios);
		}

		[TearDown]
		public void AfterTest()
		{
			StateHolderProxyHelper.ClearStateHolder();
			Thread.CurrentPrincipal = null;
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
			var principal = new TeleoptiPrincipalForLegacyFactory().MakePrincipal(loggedOnPerson, dataSource,
				BusinessUnitFactory.BusinessUnitUsedInTest, null);
			Thread.CurrentPrincipal = principal;

			StateHolderProxyHelper.ClearStateHolder();
			StateHolder.Initialize(new FakeState(), new MessageBrokerCompositeDummy());
		}
	}

#pragma warning restore 0649
}