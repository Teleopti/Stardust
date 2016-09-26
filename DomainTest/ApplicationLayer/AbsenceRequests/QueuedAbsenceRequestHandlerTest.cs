using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{

	[DomainTest]
	[TestFixture]
	public class QueuedAbsenceRequestHandlerTest : ISetup
	{
		public QueuedAbsenceRequestHandler Target;
		public QueuedAbsenceRequestFastIntradayHandler TargetFast;

		public FakePersonRequestRepository PersonRequestRepository;
		public FakeQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public FakePersonRepository PersonRepository;
		
		public FakeCurrentScenario CurrentScenario;
		public FakeEventPublisher EventPublisher;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeConfigReader ConfigReader;
		public ApprovalServiceForTest ApprovalService;
		public PersonRequestAuthorizationCheckerForTest PersonRequestAuthorizationChecker;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeCurrentScenario>().For<FakeCurrentScenario>();
			system.UseTestDouble<QueuedAbsenceRequestHandler>().For<QueuedAbsenceRequestHandler>();
			system.UseTestDouble<QueuedAbsenceRequestFastIntradayHandler>().For<QueuedAbsenceRequestFastIntradayHandler>();
			system.UseTestDouble<ApprovalServiceForTest>().For<IRequestApprovalService>();
			system.UseTestDouble<FakeIntradayRequestProcessor>().For<IIntradayRequestProcessor>();

		}


		[Test]
		public void ShouldPersistNewRequestInQueue()
		{
			var reqEvent = createNewRequestEvent();
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			reqEvent.LogOnBusinessUnitId = businessUnit.Id.Value;
			BusinessUnitRepository.Add(businessUnit);
			Target.Handle(reqEvent);

			QueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldQueueCancelledRequest()
		{
			var removedEvent = createRequestPersonAbsenceRemovedEvent();
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			removedEvent.LogOnBusinessUnitId = businessUnit.Id.GetValueOrDefault();
			BusinessUnitRepository.Add(businessUnit);
			Target.Handle(removedEvent);

			QueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(1);
			QueuedAbsenceRequestRepository.LoadAll().FirstOrDefault().PersonRequest.Should().Be.EqualTo(removedEvent.PersonRequestId);
		}

		[Test]
		public void ShouldNotPutRequestOnQueueIfIntradayRequestAndStaffingCheck()
		{
			var now = DateTime.UtcNow;
			var period = new DateTimePeriod(now.AddHours(2), now.AddHours(5)); 

			var reqEvent = createNewRequestEvent(period, new StaffingThresholdValidator());
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			reqEvent.LogOnBusinessUnitId = businessUnit.Id.Value;
			BusinessUnitRepository.Add(businessUnit);
			

			TargetFast.Handle(reqEvent);
			QueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldPutRequestOnQueueIfNotIntradayRequest()
		{
			var now = DateTime.UtcNow;
			var period = new DateTimePeriod(now.AddDays(1).AddHours(2), now.AddDays(1).AddHours(5));

			var reqEvent = createNewRequestEvent(period);
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			reqEvent.LogOnBusinessUnitId = businessUnit.Id.Value;
			BusinessUnitRepository.Add(businessUnit);


			TargetFast.Handle(reqEvent);
			QueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPutRequestOnQueueIfNotStaffingCheck()
		{
			var now = DateTime.UtcNow;
			var period = new DateTimePeriod(now.AddDays(1).AddHours(2), now.AddDays(1).AddHours(5));

			var reqEvent = createNewRequestEvent(period);
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			reqEvent.LogOnBusinessUnitId = businessUnit.Id.Value;
			BusinessUnitRepository.Add(businessUnit);


			TargetFast.Handle(reqEvent);
			QueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReadStartUtcDateTimeFromAppSettingIfExist()
		{
			var period = new DateTimePeriod(new DateTime(2016, 3, 14, 15, 0, 0, DateTimeKind.Utc),
											new DateTime(2016, 3, 14, 17, 00, 00, DateTimeKind.Utc));

			var reqEvent = createNewRequestEvent(period, new StaffingThresholdValidator());
			IBusinessUnit businessUnit = BusinessUnitFactory.CreateWithId("something");
			reqEvent.LogOnBusinessUnitId = businessUnit.Id.Value;
			BusinessUnitRepository.Add(businessUnit);

			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-14 08:00");

			TargetFast.Handle(reqEvent);
			QueuedAbsenceRequestRepository.LoadAll().Count.Should().Be.EqualTo(0);
		}


		private RequestPersonAbsenceRemovedEvent createRequestPersonAbsenceRemovedEvent()
		{
			var period = new DateTimePeriod(new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc),
											new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc));

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence, new AbsenceRequestNoneValidator());
			var person = createAndSetupPerson(period, workflowControlSet);

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


		private NewAbsenceRequestCreatedEvent createNewRequestEvent()
		{
			return createNewRequestEvent(new AbsenceRequestNoneValidator());
		}

		private NewAbsenceRequestCreatedEvent createNewRequestEvent(IAbsenceRequestValidator validator)
		{
			return createNewRequestEvent(new DateTimePeriod(new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc),
															new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc)), validator);
		}

		private NewAbsenceRequestCreatedEvent createNewRequestEvent(DateTimePeriod dateTimePeriod)
		{
			return createNewRequestEvent(dateTimePeriod, new AbsenceRequestNoneValidator());
		}


		private NewAbsenceRequestCreatedEvent createNewRequestEvent(DateTimePeriod dateTimePeriod, IAbsenceRequestValidator validator)
		{
			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence, validator);
			var person = createAndSetupPerson(dateTimePeriod, workflowControlSet);

			var request = createAbsenceRequest(person, absence, dateTimePeriod);

			var newAbsenceCreatedEvent = new NewAbsenceRequestCreatedEvent()
			{
				PersonRequestId = request.Id.Value
			};
			return newAbsenceCreatedEvent;
		}

		private IPerson createAndSetupPerson(DateTimePeriod period, IWorkflowControlSet workflowControlSet)
		{
			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Add(person);

			//var assignmentOne = createAssignment(person, period, CurrentScenario);
			//ScheduleRepository.Set(new IScheduleData[] {assignmentOne});

			person.WorkflowControlSet = workflowControlSet;

			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(IAbsence absence, IAbsenceRequestValidator validator)
		{
			var workflowControlSet = new WorkflowControlSet {AbsenceRequestWaitlistEnabled = false};
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

		//private IPersonAssignment createAssignment(IPerson person, DateTimePeriod period, ICurrentScenario currentScenario)
		//{
		//	period = period.ChangeStartTime(TimeSpan.FromSeconds(-period.StartDateTime.Second));
		//	period = period.ChangeEndTime(TimeSpan.FromSeconds(-period.EndDateTime.Second));
		//	return PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(
		//		currentScenario.Current(),
		//		person,
		//		period);
		//}


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
