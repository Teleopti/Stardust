﻿using System;
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

    public interface IDerivedPersistableScheduleData : IPersistableScheduleData
    {
        
    } 

    [TestFixture]
    public class ScheduleScreenDerivedScheduleDataTest
    {
        private MockRepository _mocks;
        private ScheduleScreenRefresher _target;
        private IScheduleRepository _scheduleRepository;
        private IOwnMessageQueue _messageQueueUpdater;
        private List<IEventMessage> _messages;
        private IScheduleDictionary _scheduleDictionary;
        private IUpdateScheduleDataFromMessages _scheduleDataUpdater;
        private IEventMessage _refreshedDerivedFromScheduleDataMessage;
        private Guid _refreshedDerivedFromScheduleDataEntityId;
        private IDerivedPersistableScheduleData _refreshedDerivedFromScheduleDataEntity;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _scheduleRepository = _mocks.DynamicMock<IScheduleRepository>();
            _messageQueueUpdater = _mocks.DynamicMock<IOwnMessageQueue>();
            _scheduleDataUpdater = _mocks.DynamicMock<IUpdateScheduleDataFromMessages>();

            _refreshedDerivedFromScheduleDataEntityId = Guid.NewGuid();
            _refreshedDerivedFromScheduleDataEntity = _mocks.Stub<IDerivedPersistableScheduleData>();
            Expect.Call(_refreshedDerivedFromScheduleDataEntity.Id).Return(_refreshedDerivedFromScheduleDataEntityId).Repeat.Any();

            _refreshedDerivedFromScheduleDataMessage = _mocks.Stub<IEventMessage>();
            _refreshedDerivedFromScheduleDataMessage.InterfaceType = typeof(IPersonAssignment);
            _refreshedDerivedFromScheduleDataMessage.DomainObjectId = _refreshedDerivedFromScheduleDataEntityId;
            _refreshedDerivedFromScheduleDataMessage.DomainUpdateType = DomainUpdateType.Update;
            _messages = new List<IEventMessage>
                        {
                            _refreshedDerivedFromScheduleDataMessage
                        };

            _scheduleDictionary = _mocks.DynamicMock<IScheduleDictionary>();
            Expect.Call(_scheduleDictionary.DifferenceSinceSnapshot()).Return(new DifferenceCollection<IPersistableScheduleData>());

            MakeTarget();
        }

        private void MakeTarget()
        {
            _target = new ScheduleScreenRefresher(_messageQueueUpdater, new ScheduleRefresher(MockRepository.GenerateMock<IPersonAssignmentRepository>(), MockRepository.GenerateMock<IPersonRepository>(), _scheduleDataUpdater), new ScheduleDataRefresher(_scheduleRepository, _scheduleDataUpdater), new MeetingRefresher(null), new PersonRequestRefresher(null));
        }

        [Test]
        public void ShouldUpdateInsertRefreshedDerivedScheduleDataEntities()
        {
            _scheduleDataUpdater = _mocks.DynamicMock<IUpdateScheduleDataFromMessages>();

            MakeTarget();

            Expect.Call(_scheduleDataUpdater.UpdateInsertScheduleData(_refreshedDerivedFromScheduleDataMessage)).Return(_refreshedDerivedFromScheduleDataEntity);

            _mocks.ReplayAll();

            _target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), new List<PersistConflictMessageState>());

            _mocks.VerifyAll();
        }

    }
}