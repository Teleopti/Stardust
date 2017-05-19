using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;
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
    public class RunRequestWaitlistEventHandlerTest : ISetup
    {
        public RunRequestWaitlistEventHandler Target;
        public MessageBrokerCompositeDummy Sender;
	    public MutableNow Now;
	    public FakeScenarioRepository ScenarioRepository;
	    public FakePersonAssignmentRepository PersonAssignmentRepository;
	    public FakePersonRequestRepository PersonRequestRepository;
	    public DenyRequestCommandHandler DenyRequestCommandHandler;
	    public FakeLoggedOnUser LoggedOnUser;


	    public void Setup(ISystem system, IIocConfiguration configuration)
	    {
		   system.UseTestDouble<MessageBrokerCompositeDummy>().For<IMessageBrokerComposite>();
		   system.UseTestDouble<RunRequestWaitlistEventHandler>().For<IHandleEvent<RunRequestWaitlistEvent>>(); //skip attributes
		   system.UseTestDouble<DenyRequestCommandHandler>().For<IHandleCommand<DenyRequestCommand>>();
		   system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
	    }

		[Test]
		public void ShouldTriggerWaitlistProcessing()
		{
			Now.Is("2017-05-19 08:00");
		
			var scenario = ScenarioRepository.Has("scnearioName");
			var absence = AbsenceFactory.CreateAbsence("Holiday");

			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod
			{
				Absence = absence,
				PersonAccountValidator = new StaffingThresholdWithShrinkageValidator(),
				StaffingThresholdValidator = new AbsenceRequestNoneValidator(),
				Period = new DateOnlyPeriod(2010, 12, 1, 2099, 12, 2),
				OpenForRequestsPeriod = new DateOnlyPeriod(2010, 11, 1, 2099, 11, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			wfcs.AbsenceRequestWaitlistEnabled = true;
			var person = PersonFactory.CreatePerson(wfcs).WithId();
			LoggedOnUser.SetFakeLoggedOnUser(person);


			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2017, 05, 20, 12, 2017, 05, 20, 13))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);
			DenyRequestCommandHandler.Handle(new DenyRequestCommand{PersonRequestId = personRequest.Id.GetValueOrDefault()});
			personRequest.IsWaitlisted.Should().Be.True();

			Target.Handle(new RunRequestWaitlistEvent
			{
				StartTime = DateTime.MinValue.Utc(),
				EndTime = DateTime.MaxValue.Utc()
			});
			personRequest.IsWaitlisted.Should().Be.False();
			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
        public void SHouldSendMessageAfterRunRequestWaitlistEventHandling()
        {

            var startDateTime = new DateTime(2016, 3, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDateTime = new DateTime(2016, 3, 1, 23, 59, 00, DateTimeKind.Utc);
            var @event = new RunRequestWaitlistEvent
            {
                InitiatorId = Guid.Empty,
                JobName = "Run Request Waitlist",
                StartTime = startDateTime,
                EndTime = endDateTime,
                LogOnBusinessUnitId = Guid.Empty,
                LogOnDatasource = "dataSource",
                Timestamp = DateTime.UtcNow,
                CommandId = Guid.Empty
            };

            Target.Handle(@event);

            Sender.SentCount().Should().Be(1);
        }

	   
    }
}