using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.Persisters
{
    [TestFixture]
    public class ScheduleScreenRefresherTest
    {
        private MockRepository _mocks;
        private ScheduleScreenRefresher _target;
        private IScheduleRepository _scheduleRepository;
        private IOwnMessageQueue _messageQueueUpdater;
        private IScheduleDictionary _scheduleDictionary;
        private IUpdateScheduleDataFromMessages _scheduleDataUpdater;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _scheduleRepository = _mocks.DynamicMock<IScheduleRepository>();
            _messageQueueUpdater = _mocks.DynamicMock<IOwnMessageQueue>();
            _scheduleDataUpdater = _mocks.DynamicMock<IUpdateScheduleDataFromMessages>();

            _scheduleDictionary = _mocks.DynamicMock<IScheduleDictionary>();

            Expect.Call(_scheduleDictionary.DifferenceSinceSnapshot()).Return(new DifferenceCollection<IPersistableScheduleData>());

            MakeTarget();
        }

        private void MakeTarget()
        {
            _target = new ScheduleScreenRefresher(_messageQueueUpdater, new ScheduleRefresher(MockRepository.GenerateMock<IPersonRepository>(), _scheduleDataUpdater, MockRepository.GenerateMock<IPersonAssignmentRepository>(), MockRepository.GenerateMock<IPersonAbsenceRepository>()), new ScheduleDataRefresher(_scheduleRepository, _scheduleDataUpdater), new MeetingRefresher(null), new PersonRequestRefresher(null));
        }

        [Test]
        public void ShouldAlwaysReassociateData()
        {
            Expect.Call(_messageQueueUpdater.ReassociateDataWithAllPeople);

            _mocks.ReplayAll();

            _target.Refresh(_scheduleDictionary, new List<IEventMessage>(), new List<IPersistableScheduleData>(), new List<PersistConflictMessageState>(), _ => true);

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldAlwaysReportSizeOfMessageBrokerQueue()
        {
            Expect.Call(() => _messageQueueUpdater.NotifyMessageQueueSizeChange());

            _mocks.ReplayAll();

            _target.Refresh(_scheduleDictionary, new List<IEventMessage>(), new List<IPersistableScheduleData>(), new List<PersistConflictMessageState>(),_ => true);

            _mocks.VerifyAll();
        }
    }
}