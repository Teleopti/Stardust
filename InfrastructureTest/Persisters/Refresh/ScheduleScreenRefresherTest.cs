using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Refresh
{
    [TestFixture]
    public class ScheduleScreenRefresherTest
    {
        private MockRepository _mocks;
        private ScheduleScreenRefresher _target;
        private IScheduleStorage _scheduleStorage;
        private IReassociateDataForSchedules _messageQueueUpdater;
        private IScheduleDictionary _scheduleDictionary;
        private IUpdateScheduleDataFromMessages _scheduleDataUpdater;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _scheduleStorage = _mocks.DynamicMock<IScheduleStorage>();
            _messageQueueUpdater = _mocks.DynamicMock<IReassociateDataForSchedules>();
            _scheduleDataUpdater = _mocks.DynamicMock<IUpdateScheduleDataFromMessages>();

            _scheduleDictionary = _mocks.DynamicMock<IScheduleDictionary>();

			Expect.Call(_scheduleDictionary.DifferenceSinceSnapshot()).Return(new DifferenceCollection<IPersistableScheduleData>());

            MakeTarget();
        }

        private void MakeTarget()
        {
	        var mqremover = MockRepository.GenerateMock<IMessageQueueRemoval>();
					_target = new ScheduleScreenRefresher(_messageQueueUpdater, new ScheduleRefresher(MockRepository.GenerateMock<IPersonRepository>(), _scheduleDataUpdater, MockRepository.GenerateMock<IPersonAssignmentRepository>(), MockRepository.GenerateMock<IPersonAbsenceRepository>(), mqremover), new ScheduleDataRefresher(_scheduleStorage, _scheduleDataUpdater, mqremover), new MeetingRefresher(null, mqremover), new PersonRequestRefresher(null, mqremover));
        }

        [Test]
        public void ShouldAlwaysReassociateData()
        {
            Expect.Call(_messageQueueUpdater.ReassociateDataForAllPeople);

            _mocks.ReplayAll();

			_target.Refresh(_scheduleDictionary, new List<IEventMessage>(), new List<IPersistableScheduleData>(), new List<PersistConflict>(), _ => true);

            _mocks.VerifyAll();
        }
    }
}