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
    public class ScheduleScreenRefresherMeetingTest
    {
        private MockRepository _mocks;
        private ScheduleScreenRefresher _target;
        private IOwnMessageQueue _messageQueueUpdater;
        private List<IEventMessage> _messages;
        private IScheduleDictionary _scheduleDictionary;
        private Guid _meetingEntityId;
        private IMeeting _meetingEntity;
        private IEventMessage _meetingMessage;
        private IUpdateMeetingsFromMessages _meetingUpdater;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _messageQueueUpdater = _mocks.DynamicMock<IOwnMessageQueue>();
            _meetingUpdater = _mocks.DynamicMock<IUpdateMeetingsFromMessages>();

            _meetingEntityId = Guid.NewGuid();
            _meetingEntity = _mocks.Stub<IMeeting>();
            Expect.Call(_meetingEntity.Id).Return(_meetingEntityId).Repeat.Any();

            _meetingMessage = _mocks.Stub<IEventMessage>();
            _meetingMessage.InterfaceType = typeof(IMeeting);
            _meetingMessage.DomainObjectId = _meetingEntityId;
            _meetingMessage.DomainUpdateType = DomainUpdateType.Update;
            _messages = new List<IEventMessage> { _meetingMessage };

            _scheduleDictionary = _mocks.DynamicMock<IScheduleDictionary>();

            MakeTarget();
        }

        private void MakeTarget()
        {
            _target = new ScheduleScreenRefresher(_messageQueueUpdater, new ScheduleRefresher(null, null, MockRepository.GenerateMock<IUpdateScheduleDataFromMessages>()), new ScheduleDataRefresher(null, null), new MeetingRefresher(_meetingUpdater), new PersonRequestRefresher(null));
        }

        [Test]
        public void ShouldCallbackMessageHandlerForMeetingEntities()
        {
            Expect.Call(() => _meetingUpdater.UpdateMeeting(_meetingMessage));

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

            CollectionAssert.DoesNotContain(_messages, _meetingMessage);
        }
    }
}