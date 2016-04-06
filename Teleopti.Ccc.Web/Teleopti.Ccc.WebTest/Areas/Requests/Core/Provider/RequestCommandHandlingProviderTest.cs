using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
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
		public IRequestApprovalServiceFactory RequestApprovalServiceFactory;

		[Test]
		public void TargetShouldNotBeNull()
		{
			Target.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotHandleApproveCommandWithInvalidRequestId()
		{
			var affectedIds = Target.ApproveRequests(new List<Guid> { new Guid() });
			affectedIds.ToList().Count.Should().Be.EqualTo(0);
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

			requestApprovalService.Stub(x => x.ApproveAbsence(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9), person)).Return(new List<IBusinessRuleResponse>());
			Target.ApproveRequests(new List<Guid> { personRequest.Id.Value });
			requestApprovalService.AssertWasCalled(x => x.ApproveAbsence(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9), person), options => options.Repeat.AtLeastOnce());
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
			personRequest.Pending();

			requestApprovalService.Stub(x => x.ApproveAbsence(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9), person)).Return(new List<IBusinessRuleResponse>());
			var affectedIds = Target.ApproveRequests(new List<Guid> { personRequest.Id.Value });

			affectedIds.ToList().Count.Should().Be.EqualTo(1);
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

			requestApprovalService.Stub(x => x.ApproveAbsence(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9), person)).Return(new List<IBusinessRuleResponse>());
			var affectedIds = Target.ApproveRequests(new List<Guid> { personRequest1.Id.Value, personRequest2.Id.Value });

			affectedIds.ToList().Count.Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldNotHandleDenyCommandWithInvalidRequestId()
		{
			var affectedIds = Target.DenyRequests(new List<Guid> { new Guid() });
			affectedIds.ToList().Count.Should().Be.EqualTo(0);
		}


		[Test]
		public void ShouldDenyAbsenceRequestWithPendingStatus()
		{

			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");

			var personRequest = createNewAbsenceRequest(person, absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			personRequest.Pending();

			var affectedIds = Target.DenyRequests(new List<Guid> { personRequest.Id.Value });

			affectedIds.ToList().Count.Should().Be.EqualTo(1);
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

			var affectedIds = Target.DenyRequests(new List<Guid> { personRequest1.Id.Value, personRequest2.Id.Value });

			affectedIds.ToList().Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldManuallyDenyWaitlistRequest()
		{
			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");

			person.WorkflowControlSet = createWorkFlowControlSet(new DateTime(2016, 2, 1, 10, 0, 0, DateTimeKind.Utc), new DateTime(2016, 4, 1, 23, 00, 00, DateTimeKind.Utc), absence);
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

			person.WorkflowControlSet = createWorkFlowControlSet(new DateTime(2016, 2, 1, 10, 0, 0, DateTimeKind.Utc), DateTime.Today, absence);
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
			var absenceRequest = new AbsenceRequest(absence, requestDateTimePeriod);
			var personRequest = new PersonRequest(person, absenceRequest);

			personRequest.SetId(Guid.NewGuid());

			if (isAutoDenied)
			{

				personRequest.Deny(null, "Work Hard!", new PersonRequestAuthorizationCheckerForTest());
			}

			PersonRequestRepository.Add(personRequest);

			return personRequest;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence)
		{
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = true };

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
