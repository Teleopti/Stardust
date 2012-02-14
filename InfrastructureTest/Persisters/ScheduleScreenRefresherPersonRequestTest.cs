﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.Persisters
{
    [TestFixture]
    public class ScheduleScreenRefresherPersonRequestTest
    {
        private MockRepository _mocks;
        private ScheduleScreenRefresher _target;
        private IOwnMessageQueue _messageQueueUpdater;
        private List<IEventMessage> _messages;
        private IScheduleDictionary _scheduleDictionary;
        private IEventMessage _personRequestMessage;
        private Guid _personRequestEntityId;
        private IPersonRequest _personRequestEntity;
        private IUpdatePersonRequestsFromMessages _personRequestUpdater;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _messageQueueUpdater = _mocks.DynamicMock<IOwnMessageQueue>();
            _personRequestUpdater = _mocks.DynamicMock<IUpdatePersonRequestsFromMessages>();

            _personRequestEntityId = Guid.NewGuid();
            _personRequestEntity = _mocks.Stub<IPersonRequest>();
            Expect.Call(_personRequestEntity.Id).Return(_personRequestEntityId).Repeat.Any();

            _personRequestMessage = _mocks.Stub<IEventMessage>();
            _personRequestMessage.InterfaceType = typeof(IPersonRequest);
            _personRequestMessage.DomainObjectId = _personRequestEntityId;
            _personRequestMessage.DomainUpdateType = DomainUpdateType.Update;
            _messages = new List<IEventMessage> { _personRequestMessage};

            _scheduleDictionary = _mocks.DynamicMock<IScheduleDictionary>();

            MakeTarget();
        }

        private void MakeTarget()
        {
            _target = new ScheduleScreenRefresher(_messageQueueUpdater, new ScheduleDataRefresher(null, null), new MeetingRefresher(null), new PersonRequestRefresher(_personRequestUpdater));
        }

        [Test]
        public void ShouldCallbackMessageHandlerForMeetingEntities()
        {
            Expect.Call(() => _personRequestUpdater.UpdatePersonRequest(_personRequestMessage));

            _mocks.ReplayAll();

            _target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), new List<PersistConflictMessageState>());

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldRemoveRefreshedMeetingMessagesFromQueue()
        {
            _mocks.ReplayAll();

            _target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), new List<PersistConflictMessageState>());

            _mocks.VerifyAll();

            CollectionAssert.DoesNotContain(_messages, _personRequestMessage);
        }
    }
}