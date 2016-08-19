using System;
using System.Configuration;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{

	[DomainTest]
	[TestFixture]
	public class NewAbsenceRequestUseMultiHandlerTest : ISetup
	{
		public NewAbsenceRequestUseMultiHandler Target;

		public FakePersonRequestRepository PersonRequestRepository;
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public FakePersonRepository PersonRepository;
		public FakeScheduleDataReadScheduleStorage ScheduleRepository;
		
		public FakeCurrentScenario CurrentScenario;
		public FakeEventPublisher EventPublisher;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeScheduleDataReadScheduleStorage>().For<FakeScheduleDataReadScheduleStorage>();
			system.UseTestDouble<FakeCurrentScenario>().For<FakeCurrentScenario>();
			system.UseTestDouble<NewAbsenceRequestUseMultiHandler>().For<NewAbsenceRequestUseMultiHandler>();
			system.UseTestDouble(new FakeConfigReader("NumberOfAbsenceRequestsToBulkProcess", "2")).For<IConfigReader>();
		}


		[Test]
		public void ShouldPersistNewRequestInQueue()
		{
			var reqEvent = createNewRequestEvent();
			Target.Handle(reqEvent);

			QueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(1);
		}


		//[Test]
		//public void ShouldPickJobsFromQueueAndSendMultiRequestEvent()
		//{
		//	var requestEvent1 = createNewRequestEvent();
		//	var requestEvent2 = createNewRequestEvent();

		//	Target.Handle(requestEvent1);
		//	Target.Handle(requestEvent2);

		//	EventPublisher.PublishedEvents.Count().Should().Be.EqualTo(1);
		//	Assert.IsInstanceOf<NewMultiAbsenceRequestsCreatedEvent>(EventPublisher.PublishedEvents.FirstOrDefault());
		//}


		//[Test]
		//public void ShouldRemoveJobsFromQueueWhenSentAsMultiRequestEvent()
		//{
		//	var requestEvent1 = createNewRequestEvent();
		//	var requestEvent2 = createNewRequestEvent();

		//	Target.Handle(requestEvent1);
		//	Target.Handle(requestEvent2);

		//	QueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(0);
		//}


		



		private NewAbsenceRequestCreatedEvent createNewRequestEvent()
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence);
			var person = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

			var request = createAbsenceRequest(person, absence, new DateTimePeriod(startDateTime, endDateTime));

			var newAbsenceCreatedEvent = new NewAbsenceRequestCreatedEvent()
			{
				PersonRequestId = request.Id.Value
			};
			return newAbsenceCreatedEvent;
		}

		private IPerson createAndSetupPerson(DateTime startDateTime, DateTime endDateTime, IWorkflowControlSet workflowControlSet)
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			var assignmentOne = createAssignment(person, startDateTime, endDateTime, CurrentScenario);
			ScheduleRepository.Set(new IScheduleData[] {assignmentOne});

			person.WorkflowControlSet = workflowControlSet;

			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(DateTime startDate, DateTime endDate, IAbsence absence)
		{
			var workflowControlSet = new WorkflowControlSet {AbsenceRequestWaitlistEnabled = false};
			workflowControlSet.SetId(Guid.NewGuid());

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;
		}

		private IPersonAssignment createAssignment(IPerson person, DateTime startDate, DateTime endDate, ICurrentScenario currentScenario)
		{
			return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
				currentScenario.Current(),
				person,
				new DateTimePeriod(startDate, endDate));
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

	public class FakePersonRequest : PersonRequest
	{
		public FakePersonRequest(IPerson person, IRequest request) : base(person, request)
		{
		}

		public void SetCreated(DateTime created)
		{
			CreatedOn = created;
		}
	}
}
