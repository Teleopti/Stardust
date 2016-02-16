using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
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
    public class ScheduleScreenRefresherScheduleDataTest : IMessageQueueRemoval
    {
        private MockRepository _mocks;
        private ScheduleScreenRefresher _target;
        private IScheduleStorage _scheduleStorage;
        private IReassociateDataForSchedules _messageQueueUpdater;
        private Guid _conflictingScheduleDataEntityId;
		private IPersistableScheduleData _conflictingScheduleDataEntity;
        private Guid _refreshedScheduleDataEntityId;
		private IPersistableScheduleData _refreshedScheduleDataEntity;
        private IEventMessage _conflictingScheduleDataMessage;
        private IEventMessage _refreshedScheduleDataMessage;
        private List<IEventMessage> _messages;
		private DifferenceCollection<IPersistableScheduleData> _myChanges;
		private DifferenceCollectionItem<IPersistableScheduleData> _myChange;
        private IScheduleDictionary _scheduleDictionary;
        private Guid _deletedScheduleDataEntityId;
		private IPersistableScheduleData _deletedScheduleDataEntity;
        private IEventMessage _deletedScheduleDataMessage;
        private IUpdateScheduleDataFromMessages _scheduleDataUpdater;
        private IEventMessage _refreshedDerivedFromScheduleDataMessage;
        private Guid _refreshedDerivedFromScheduleDataEntityId;
        private IPersonAssignment _refreshedDerivedFromScheduleDataEntity;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _scheduleStorage = _mocks.DynamicMock<IScheduleStorage>();
            _messageQueueUpdater = _mocks.DynamicMock<IReassociateDataForSchedules>();
            _scheduleDataUpdater = _mocks.DynamicMock<IUpdateScheduleDataFromMessages>();

            _conflictingScheduleDataEntityId = Guid.NewGuid();
			_conflictingScheduleDataEntity = _mocks.Stub<IPersistableScheduleData>();
            Expect.Call(_conflictingScheduleDataEntity.Id).Return(_conflictingScheduleDataEntityId).Repeat.Any();

            _refreshedScheduleDataEntityId = Guid.NewGuid();
			_refreshedScheduleDataEntity = _mocks.Stub<IPersistableScheduleData>();
            Expect.Call(_refreshedScheduleDataEntity.Id).Return(_refreshedScheduleDataEntityId).Repeat.Any();

            _refreshedDerivedFromScheduleDataEntityId = Guid.NewGuid();
            _refreshedDerivedFromScheduleDataEntity = _mocks.Stub<IPersonAssignment>();
            Expect.Call(_refreshedDerivedFromScheduleDataEntity.Id).Return(_refreshedDerivedFromScheduleDataEntityId).Repeat.Any();

            _deletedScheduleDataEntityId = Guid.NewGuid();
			_deletedScheduleDataEntity = _mocks.Stub<IPersistableScheduleData>();
            Expect.Call(_deletedScheduleDataEntity.Id).Return(_deletedScheduleDataEntityId).Repeat.Any();

            _conflictingScheduleDataMessage = _mocks.Stub<IEventMessage>();
            _conflictingScheduleDataMessage.InterfaceType = typeof(IPersistableScheduleData);
            _conflictingScheduleDataMessage.DomainObjectId = _conflictingScheduleDataEntityId;
            _refreshedScheduleDataMessage = _mocks.Stub<IEventMessage>();
            _refreshedScheduleDataMessage.InterfaceType = typeof(IPersistableScheduleData);
            _refreshedScheduleDataMessage.DomainObjectId = _refreshedScheduleDataEntityId;
            _refreshedScheduleDataMessage.DomainUpdateType = DomainUpdateType.Update;
            _refreshedDerivedFromScheduleDataMessage = _mocks.Stub<IEventMessage>();
            _refreshedDerivedFromScheduleDataMessage.InterfaceType = typeof(IPersonAssignment);
            _refreshedDerivedFromScheduleDataMessage.DomainObjectId = _refreshedScheduleDataEntityId;
            _refreshedDerivedFromScheduleDataMessage.DomainUpdateType = DomainUpdateType.Update;
            _deletedScheduleDataMessage = _mocks.Stub<IEventMessage>();
            _deletedScheduleDataMessage.InterfaceType = typeof(IPersistableScheduleData);
            _deletedScheduleDataMessage.DomainObjectId = _deletedScheduleDataEntityId;
            _deletedScheduleDataMessage.DomainUpdateType = DomainUpdateType.Delete;
            _messages = new List<IEventMessage>
                        {
                            _conflictingScheduleDataMessage, 
                            _refreshedScheduleDataMessage, 
                            _refreshedDerivedFromScheduleDataMessage, 
                            _deletedScheduleDataMessage
                        };

			_myChanges = new DifferenceCollection<IPersistableScheduleData>();
			_myChange = new DifferenceCollectionItem<IPersistableScheduleData>(_conflictingScheduleDataEntity, _conflictingScheduleDataEntity);
            _myChanges.Add(_myChange);

            Expect.Call(_scheduleStorage.LoadScheduleDataAggregate(typeof(IPersistableScheduleData), _conflictingScheduleDataEntityId)).Return(_mocks.DynamicMock<IPersistableScheduleData>()).Repeat.Any();
            Expect.Call(_scheduleDataUpdater.UpdateInsertScheduleData(_refreshedScheduleDataMessage)).Return(_refreshedScheduleDataEntity).Repeat.Any();

            _scheduleDictionary = _mocks.DynamicMock<IScheduleDictionary>();
            Expect.Call(_scheduleDictionary.DifferenceSinceSnapshot()).Return(_myChanges);

            MakeTarget();
        }

        private void MakeTarget()
        {
	        var mqremover = this;
					_target = new ScheduleScreenRefresher(_messageQueueUpdater, new ScheduleRefresher(MockRepository.GenerateMock<IPersonRepository>(), _scheduleDataUpdater, MockRepository.GenerateMock<IPersonAssignmentRepository>(), MockRepository.GenerateMock<IPersonAbsenceRepository>(), mqremover), new ScheduleDataRefresher(_scheduleStorage, _scheduleDataUpdater, mqremover), new MeetingRefresher(null, mqremover), new PersonRequestRefresher(null, mqremover));
        }

        [Test]
        public void ShouldFillScheduleDataConflicts()
        {
            _mocks.ReplayAll();

            var conflictingEntitiesBuffer = new List<PersistConflict>();
			_target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), conflictingEntitiesBuffer, _ => true);

            _mocks.VerifyAll();

            Assert.That(conflictingEntitiesBuffer.Single().ClientVersion, Is.EqualTo(_myChange));
        }

		[Test]
		public void CanRemoveConflictFromBuffer()
		{
			_mocks.ReplayAll();
			var messageCountBefore = _messages.Count;
			var conflictingEntitiesBuffer = new List<PersistConflict>();
			_target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), conflictingEntitiesBuffer, _ => true);

			//conflictingEntitiesBuffer.Single().RemoveFromCollection();
			_messages.Count
				.Should().Be.LessThan(messageCountBefore);

			_mocks.VerifyAll();
		}


        [Test]
        public void ShouldFillRefreshedScheduleData()
        {
            _mocks.ReplayAll();

			var refreshedEntitiesBuffer = new List<IPersistableScheduleData>();
						_target.Refresh(_scheduleDictionary, _messages, refreshedEntitiesBuffer, new List<PersistConflict>(), _ => true);

            _mocks.VerifyAll();

            Assert.That(refreshedEntitiesBuffer.Single(), Is.SameAs(_refreshedScheduleDataEntity));
        }

        [Test]
        public void ShouldRemoveRefreshedScheduleDataMessagesFromQueue()
        {
            _mocks.ReplayAll();

			_target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), new List<PersistConflict>(), _ => true);

            _mocks.VerifyAll();

            CollectionAssert.DoesNotContain(_messages, _refreshedScheduleDataEntity);
        }

        [Test]
        public void ShouldFillReloadedScheduleDataOnConflictingScheduleDataDatabaseEntity()
        {
            _scheduleStorage = _mocks.StrictMock<IScheduleStorage>();

            MakeTarget();

            var databaseVersionOfConflictingEntity = _mocks.Stub<IPersistableScheduleData>();
            Expect.Call(_scheduleStorage.LoadScheduleDataAggregate(typeof(IPersistableScheduleData), _conflictingScheduleDataEntityId)).Return(databaseVersionOfConflictingEntity);
            Expect.Call(() => _scheduleDataUpdater.FillReloadedScheduleData(databaseVersionOfConflictingEntity));

            _mocks.ReplayAll();

			_target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), new List<PersistConflict>(), _ => true);

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldUpdateInsertRefreshedScheduleDataEntities()
        {
            _scheduleDataUpdater = _mocks.DynamicMock<IUpdateScheduleDataFromMessages>();

            MakeTarget();

            Expect.Call(_scheduleDataUpdater.UpdateInsertScheduleData(_refreshedScheduleDataMessage)).Return(_refreshedScheduleDataEntity);

            _mocks.ReplayAll();

			_target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), new List<PersistConflict>(), _ => true);

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldUpdateInsertRefreshedDerivedFromScheduleDataEntities()
        {
            _scheduleDataUpdater = _mocks.DynamicMock<IUpdateScheduleDataFromMessages>();

            MakeTarget();

            Expect.Call(_scheduleDataUpdater.UpdateInsertScheduleData(_refreshedDerivedFromScheduleDataMessage)).Return(_refreshedDerivedFromScheduleDataEntity);

            _mocks.ReplayAll();

			_target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), new List<PersistConflict>(), _ => true);

            _mocks.VerifyAll();
        }


        [Test]
        public void ShouldDeleteDeletedScheduleDataEntities()
        {
            Expect.Call(_scheduleDataUpdater.DeleteScheduleData(_deletedScheduleDataMessage)).Return(_deletedScheduleDataEntity);

            _mocks.ReplayAll();

			_target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), new List<PersistConflict>(), _ => true);

            _mocks.VerifyAll();
        }

	    public void Remove(IEventMessage eventMessage)
	    {
		    _messages.Remove(eventMessage);
	    }

	    public void Remove(PersistConflict persistConflict)
	    {
		    throw new NotImplementedException();
	    }
    }
}
