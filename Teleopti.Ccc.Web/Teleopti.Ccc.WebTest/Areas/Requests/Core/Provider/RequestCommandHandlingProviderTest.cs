using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Requests.Core.Provider;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Ccc.WebTest.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.Provider
{
#pragma warning disable 0649
	[TestFixture, RequestsTest]
	class RequestCommandHandlingProviderTest
	{
		public IRequestCommandHandlingProvider Target;

		public ICurrentScenario Scenario;
		public IScheduleRepository ScheduleRepository;
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
			var affectedIds = Target.ApproveRequests(new List<Guid> {new Guid()});
			affectedIds.ToList().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldInvokeApproveAbsenceFromRequestApprovalServiceWithValidAbsenceRequest()
		{
			var scheduleDictionary = new FakeScheduleDictionary();

			var requestApprovalService = RequestApprovalServiceFactory.MakeRequestApprovalServiceScheduler(scheduleDictionary,
				Scenario.Current());

			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var personRequest = new PersonRequest(person, absenceRequest);
			personRequest.SetId(Guid.NewGuid());

			var personRequestRepostitory = PersonRequestRepository as FakePersonRequestRepository;
			personRequestRepostitory.Add(personRequest);

			requestApprovalService.Stub(x => x.ApproveAbsence(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9), person)).Return(new List<IBusinessRuleResponse>());
			Target.ApproveRequests(new List<Guid> {personRequest.Id.Value});
			requestApprovalService.AssertWasCalled(x => x.ApproveAbsence(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9), person), options => options.Repeat.AtLeastOnce());			
		}


		[Test]
		public void ShouldApproveAbsenceRequestWithPendingStatus()
		{
			var scheduleDictionary = new FakeScheduleDictionary();

			var requestApprovalService = RequestApprovalServiceFactory.MakeRequestApprovalServiceScheduler(scheduleDictionary,
				Scenario.Current());

			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var personRequest = new PersonRequest(person, absenceRequest);
			personRequest.SetId(Guid.NewGuid());

			personRequest.Pending();

			var personRequestRepostitory = PersonRequestRepository as FakePersonRequestRepository;
			personRequestRepostitory.Add(personRequest);

			requestApprovalService.Stub(x => x.ApproveAbsence(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9), person)).Return(new List<IBusinessRuleResponse>());
			var affectedIds = Target.ApproveRequests(new List<Guid> { personRequest.Id.Value });

			affectedIds.ToList().Count.Should().Be.EqualTo(1);
		}


		[Test]
		public void ShouldApproveAllAbsenceRequestsWithPendingStatus()
		{
			var scheduleDictionary = new FakeScheduleDictionary();

			var requestApprovalService = RequestApprovalServiceFactory.MakeRequestApprovalServiceScheduler(scheduleDictionary,
				Scenario.Current());

			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");
			var absenceRequest1 = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var personRequest1 = new PersonRequest(person, absenceRequest1);
			personRequest1.SetId(Guid.NewGuid());

			var absenceRequest2 = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var personRequest2 = new PersonRequest(person, absenceRequest2);
			personRequest2.SetId(Guid.NewGuid());

			personRequest1.Pending();
			personRequest2.Pending();

			var personRequestRepostitory = PersonRequestRepository as FakePersonRequestRepository;
			personRequestRepostitory.Add(personRequest1);
			personRequestRepostitory.Add(personRequest2);

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
			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var personRequest = new PersonRequest(person, absenceRequest);
			personRequest.SetId(Guid.NewGuid());

			personRequest.Pending();

			var personRequestRepostitory = PersonRequestRepository as FakePersonRequestRepository;
			personRequestRepostitory.Add(personRequest);
			
			var affectedIds = Target.DenyRequests(new List<Guid> { personRequest.Id.Value });

			affectedIds.ToList().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldDenyAllAbsenceRequestsWithPendingStatus()
		{
			

			var absence = AbsenceFactory.CreateAbsence("absence");
			var person = PersonFactory.CreatePerson("tester");
			var absenceRequest1 = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var personRequest1 = new PersonRequest(person, absenceRequest1);
			personRequest1.SetId(Guid.NewGuid());

			var absenceRequest2 = new AbsenceRequest(absence, new DateTimePeriod(2015, 10, 3, 2015, 10, 9));
			var personRequest2 = new PersonRequest(person, absenceRequest2);
			personRequest2.SetId(Guid.NewGuid());

			personRequest1.Pending();
			personRequest2.Pending();

			var personRequestRepostitory = PersonRequestRepository as FakePersonRequestRepository;
			personRequestRepostitory.Add(personRequest1);
			personRequestRepostitory.Add(personRequest2);

			var affectedIds = Target.DenyRequests(new List<Guid> { personRequest1.Id.Value, personRequest2.Id.Value });

			affectedIds.ToList().Count.Should().Be.EqualTo(2);
		}

	}
#pragma warning restore 0649
}
