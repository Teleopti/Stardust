using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class RequestPersonAbsenceRemovedEventHandlerTest : ISetup
	{
		public RequestPersonAbsenceRemovedEventHandler Target;

		public FakePersonRequestRepository PersonRequestRepository;
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public FakePersonRepository PersonRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public ApprovalServiceForTest ApprovalService;
		public PersonRequestAuthorizationCheckerForTest PersonRequestAuthorizationChecker;
		public FakeWorkflowControlSetRepository WorkflowControlSetRepository;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<ApprovalServiceForTest>().For<IRequestApprovalService>();
		}

		[Test]
		public void ShouldQueueCancelledRequest()
		{
			var removedEvent = createRequestPersonAbsenceRemovedEvent(true);
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			removedEvent.LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault();
			BusinessUnitRepository.Add(businessUnit);
			Target.Handle(removedEvent);

			QueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(1);
			QueuedAbsenceRequestRepository.LoadAll().FirstOrDefault().PersonRequest.Should().Be.EqualTo(removedEvent.PersonRequestId);
		}

		[Test]
		public void ShouldNotQueueCancelledRequestIfNoWaitlistIsConfigured()
		{
			var removedEvent = createRequestPersonAbsenceRemovedEvent(false);
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			removedEvent.LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault();
			BusinessUnitRepository.Add(businessUnit);
			Target.Handle(removedEvent);

			QueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldNotQueueIfAnyWorkflowControlSetIsWaitlisted()
		{
			WorkflowControlSetRepository.Add(new WorkflowControlSet() {AbsenceRequestWaitlistEnabled = true});
			var removedEvent = createRequestPersonAbsenceRemovedEvent(false);
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			removedEvent.LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault();
			BusinessUnitRepository.Add(businessUnit);
			Target.Handle(removedEvent);

			QueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(1);
		}

		private RequestPersonAbsenceRemovedEvent createRequestPersonAbsenceRemovedEvent(bool enableWaitlist = false)
		{
			var period = new DateTimePeriod(new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc),
											new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence, new AbsenceRequestNoneValidator(),enableWaitlist);
			WorkflowControlSetRepository.Add(workflowControlSet);
			var person = createAndSetupPerson(workflowControlSet);

			var request = createAbsenceRequest(person, absence, period);
			request.Pending();
			request.Approve(ApprovalService, PersonRequestAuthorizationChecker);
			request.Cancel(PersonRequestAuthorizationChecker);
			var requestPersonAbsenceRemovedEvent = new RequestPersonAbsenceRemovedEvent()
			{
				PersonRequestId = request.Id.GetValueOrDefault(),
				StartDateTime = period.StartDateTime,
				EndDateTime = period.EndDateTime
			};
			return requestPersonAbsenceRemovedEvent;
		}

		private IPerson createAndSetupPerson(IWorkflowControlSet workflowControlSet)
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			person.WorkflowControlSet = workflowControlSet;

			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(IAbsence absence, IAbsenceRequestValidator validator, bool enableWaitlist = false)
		{
			var workflowControlSet = new WorkflowControlSet { AbsenceRequestWaitlistEnabled = enableWaitlist };
			workflowControlSet.SetId(Guid.NewGuid());

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2016, 01, 01), new DateOnly(2016, 12, 31));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				StaffingThresholdValidator = validator,
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;
		}

		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			var personRequest = new FakePersonRequest(person, new AbsenceRequest(absence, requestDateTimePeriod));

			personRequest.SetId(Guid.NewGuid());
			personRequest.SetCreated(new DateTime(2016, 2, 20, 0, 0, 0, DateTimeKind.Utc));
			PersonRequestRepository.Add(personRequest);

			return personRequest;
		}

	}
}
