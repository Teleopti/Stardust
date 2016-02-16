using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Refresh
{
    [TestFixture]
	public class ScheduleScreenRefresherPersonRequestTest : IMessageQueueRemoval
    {
        private MockRepository _mocks;
        private ScheduleScreenRefresher _target;
        private IReassociateDataForSchedules _messageQueueUpdater;
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

            _messageQueueUpdater = _mocks.DynamicMock<IReassociateDataForSchedules>();
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
            _target = new ScheduleScreenRefresher(_messageQueueUpdater, new ScheduleRefresher(null, MockRepository.GenerateMock<IUpdateScheduleDataFromMessages>(), null, null, this), new ScheduleDataRefresher(null, null, this), new MeetingRefresher(null, this), new PersonRequestRefresher(_personRequestUpdater, this));
        }

        [Test]
        public void ShouldCallbackMessageHandlerForMeetingEntities()
        {
            Expect.Call(() => _personRequestUpdater.UpdatePersonRequest(_personRequestMessage));

            _mocks.ReplayAll();

			_target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), new List<PersistConflict>(), _ => true);

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldRemoveRefreshedMeetingMessagesFromQueue()
        {
            _mocks.ReplayAll();

			_target.Refresh(_scheduleDictionary, _messages, new List<IPersistableScheduleData>(), new List<PersistConflict>(), _ => true);

            _mocks.VerifyAll();

            CollectionAssert.DoesNotContain(_messages, _personRequestMessage);
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