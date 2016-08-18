using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[TestFixture]
	class NewAbsenceRequestUseMultiHandlerTest
	{
		private NewAbsenceRequestUseMultiHandler _target;

		private IPersonRequestRepository _personRequestRepository;
		private IQueuedAbsenceRequestRepository _queuedAbsenceRequestRepository;
		private IPersonRepository _personRepository;
		private FakeScheduleDataReadScheduleStorage _scheduleRepository;

		private readonly ICurrentScenario _currentScenario = new FakeCurrentScenario();
		

		[SetUp]
		public void Setup()
		{
			_personRequestRepository = new FakePersonRequestRepository();
			_queuedAbsenceRequestRepository = new FakeQueuedAbsenceRequestRepository();
			_personRepository = new FakePersonRepository();
			_scheduleRepository = new FakeScheduleDataReadScheduleStorage();

			_target = new NewAbsenceRequestUseMultiHandler(_personRequestRepository, _queuedAbsenceRequestRepository);
		}

		[Test]
		public void ShouldPersistNewRequestInQueue()
		{
			var req = createRequest();

			var newAbsenceCreatedEvent = new NewAbsenceRequestCreatedEvent()
			{
				PersonRequestId = req.Id.Value
			};

			_target.Handle(newAbsenceCreatedEvent);

			_queuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(1);
		}

		private PersonRequest createRequest()
		{
			var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
			var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(new DateTime(2016, 01, 01), new DateTime(2016, 12, 31), absence);
			var person = createAndSetupPerson(startDateTime, endDateTime, workflowControlSet);

			var request = createAbsenceRequest(person, absence, new DateTimePeriod(startDateTime, endDateTime));
			return request;
		}

		private IPerson createAndSetupPerson(DateTime startDateTime, DateTime endDateTime, IWorkflowControlSet workflowControlSet)
		{
			var person = PersonFactory.CreatePersonWithId();
			_personRepository.Add(person);

			var assignmentOne = createAssignment(person, startDateTime, endDateTime, _currentScenario);
			_scheduleRepository.Set(new IScheduleData[] {assignmentOne});

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
			_personRequestRepository.Add(personRequest);

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
