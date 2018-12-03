using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class RequestPersonAbsenceRemovedEventHandlerTest : IIsolateSystem
	{
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public FakePersonRepository PersonRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public ApprovalServiceForTest ApprovalService;
		public PersonRequestAuthorizationCheckerForTest PersonRequestAuthorizationChecker;
		public FakeWorkflowControlSetRepository WorkflowControlSetRepository;
		public FakeTenants FakeTenants;
		public FakeEventPublisher Publisher;
		public MutableNow Now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ApprovalServiceForTest>().For<IRequestApprovalService>();
			isolate.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		[Test]
		public void ShouldQueueCancelledRequest()
		{
			handle(true);

			QueuedAbsenceRequestRepository.LoadAll().Count().Should().Be.EqualTo(1);			
		}

		[Test]
		public void ShouldNotQueueCancelledRequestIfNoWaitlistIsConfigured()
		{
			handle(); 

			QueuedAbsenceRequestRepository.LoadAll().Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldQueueIfAnyWorkflowControlSetIsWaitlisted()
		{
			addWaitlistEnabledWorkFlowControlSet();

			handle();

			QueuedAbsenceRequestRepository.LoadAll().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotRunWaitlistCommand()
		{
			addWaitlistEnabledWorkFlowControlSet();

			handle();

			Publisher.PublishedEvents.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldSplitPeriodInDays()
		{
			var period = new DateTimePeriod(new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 3, 7, 23, 59, 00, DateTimeKind.Utc));

			var removedEvent = createRequestPersonAbsenceRemovedEvent(true,period);
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			removedEvent.LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault();
			BusinessUnitRepository.Add(businessUnit);
			createHandler().Handle(removedEvent);
			QueuedAbsenceRequestRepository.LoadAll().Count().Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldNotAddPlaceholderForPastDays()
		{
			Now.Is("2016-03-05");
			var period = new DateTimePeriod(new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 3, 7, 23, 59, 00, DateTimeKind.Utc));

			var removedEvent = createRequestPersonAbsenceRemovedEvent(true, period);
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			removedEvent.LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault();
			BusinessUnitRepository.Add(businessUnit);
			new RequestPersonAbsenceRemovedEventHandler(QueuedAbsenceRequestRepository, WorkflowControlSetRepository, Now).Handle(removedEvent);
			QueuedAbsenceRequestRepository.LoadAll().Count().Should().Be.EqualTo(3);
		}

		private void addWaitlistEnabledWorkFlowControlSet()
		{
			WorkflowControlSetRepository.Add(new WorkflowControlSet() { AbsenceRequestWaitlistEnabled = true });
		}

		private void handle(bool enableWaitlist = false)
		{
			var removedEvent = createRequestPersonAbsenceRemovedEvent(enableWaitlist);
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			removedEvent.LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault();
			BusinessUnitRepository.Add(businessUnit);
			createHandler().Handle(removedEvent);
		}

		private RequestPersonAbsenceRemovedEventHandler createHandler()
		{
			Now.Is("2016-03-01");
			Publisher.Clear();
			return new RequestPersonAbsenceRemovedEventHandler(QueuedAbsenceRequestRepository, WorkflowControlSetRepository, Now);
		}

		private RequestPersonAbsenceRemovedEvent createRequestPersonAbsenceRemovedEvent(bool enableWaitlist = false)
		{
			var period = new DateTimePeriod(new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc));
			return createRequestPersonAbsenceRemovedEvent(enableWaitlist, period);
		}

		private RequestPersonAbsenceRemovedEvent createRequestPersonAbsenceRemovedEvent(bool enableWaitlist, DateTimePeriod period)
		{
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
			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, requestDateTimePeriod)).WithId();
			
			personRequest.SetCreated(new DateTime(2016, 2, 20, 0, 0, 0, DateTimeKind.Utc));
			PersonRequestRepository.Add(personRequest);

			return personRequest;
		}

	}
}
