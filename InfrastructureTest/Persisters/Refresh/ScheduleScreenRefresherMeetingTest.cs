using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Refresh
{
    [TestFixture]
    public class ScheduleScreenRefresherMeetingTest : IMessageQueueRemoval
    {
        private MockRepository _mocks;
        private ScheduleScreenRefresher _target;
        private IReassociateDataForSchedules _messageQueueUpdater;
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

            _messageQueueUpdater = _mocks.DynamicMock<IReassociateDataForSchedules>();
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
					_target = new ScheduleScreenRefresher(_messageQueueUpdater, new ScheduleRefresher(null, MockRepository.GenerateMock<IUpdateScheduleDataFromMessages>(), null, null, this), new ScheduleDataRefresher(null, null, this), new MeetingRefresher(_meetingUpdater, this), new PersonRequestRefresher(null, this));
        }

        [Test]
        public void ShouldCallbackMessageHandlerForMeetingEntities()
        {
            Expect.Call(() => _meetingUpdater.UpdateMeeting(_meetingMessage));

            _mocks.ReplayAll();

			_target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), new List<PersistConflict>(), _ => true,true);

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldRemoveRefreshedMeetingMessagesFromQueue()
        {
            _mocks.ReplayAll();

			_target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), new List<PersistConflict>(), _ => true, true);

            _mocks.VerifyAll();

            CollectionAssert.DoesNotContain(_messages, _meetingMessage);
        }


			//move this logic out!
	    public void Remove(IEventMessage eventMessage)
	    {
		    _messages.Remove(eventMessage);
	    }

	    public void Remove(PersistConflict persistConflict)
	    {
		    var involvedId = persistConflict.InvolvedId();
		    for (var i = _messages.Count - 1; i >= 0; i--)
		    {
			    var eventMessage = _messages[i];
					if (eventMessage.DomainObjectId == involvedId)
					{
						_messages.Remove(eventMessage);
						return;
					}
		    }
	    }
    }
}