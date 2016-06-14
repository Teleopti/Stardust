using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Absence;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
    [TestFixture]
    public class RunRequestWaitlistEventHandlerTest
    {
        private IWorkflowControlSet wcs1;
        private IWorkflowControlSet wcs2;
        private IUnitOfWork uow;
        private IAbsenceRequestWaitlistProcessor processor;
        private RunRequestWaitlistEventHandler target;
        private IWorkflowControlSetRepository wcsRepository;


        private FakeMessageBrokerComposite _sender;
        private FakeCurrentUnitOfWorkFactory _unitOfWorkFactory;
        private IAbsenceRequestWaitlistProcessor _absenceRequestWaitlistProcessor;
        private IWorkflowControlSetRepository _workflowControlSetRepository;

        [SetUp]
        public void Setup()
        {
            wcs1 = new WorkflowControlSet();
            wcs2 = new WorkflowControlSet();
            wcsRepository = MockRepository.GenerateMock<IWorkflowControlSetRepository>();
            wcsRepository.Stub(x => x.LoadAll()).Return(new[] { wcs1, wcs2 });

            uow = new FakeUnitOfWork();
            var currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
            currentUnitOfWork.Stub(x => x.Current()).Return(uow);

            var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
            unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);

            var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
            currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
            _sender = new FakeMessageBrokerComposite();
            processor = MockRepository.GenerateMock<IAbsenceRequestWaitlistProcessor>();
            target = new RunRequestWaitlistEventHandler(currentUnitOfWorkFactory, processor, wcsRepository, _sender);
        }

        [Test]
        public void ShouldTriggerWaitlistProcessing()
        {
            var period = new DateTimePeriod(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
            target.Handle(new RunRequestWaitlistEvent
            {
                StartTime = period.StartDateTime,
                EndTime = period.EndDateTime
            });
            processor.AssertWasCalled(x => x.ProcessAbsenceRequestWaitlist(uow, period, wcs1));
            processor.AssertWasCalled(x => x.ProcessAbsenceRequestWaitlist(uow, period, wcs2));
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
            _sender = new FakeMessageBrokerComposite();
            var target = createRunRequestWaitlistEventHandler(_sender);

            target.Handle(@event);

            _sender.SentCount().Should().Be(1);

        }

        private IHandleEvent<RunRequestWaitlistEvent> createRunRequestWaitlistEventHandler(IMessageBrokerComposite sender)
        {
            _unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory();
            _absenceRequestWaitlistProcessor = new AbsenceRequestWaitlistProcessor(null, () => new FakeSchedulingResultStateHolder(), new AbsenceRequestWaitlistProvider(null));
            _workflowControlSetRepository = new FakeWorkflowControlSetRepository();

            var target = new RunRequestWaitlistEventHandler(_unitOfWorkFactory, _absenceRequestWaitlistProcessor, _workflowControlSetRepository, sender);
            return target;
        }
    }
}