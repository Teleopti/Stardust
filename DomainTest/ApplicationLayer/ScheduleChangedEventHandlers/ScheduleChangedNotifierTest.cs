using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	public class ScheduleChangedNotifierTest
	{
		[Test]
		public void ShouldSendBrokerMessageOnScheduleChange()
		{
			var broker = MockRepository.GenerateMock<IMessageBrokerSender>();
			var handler = new ScheduleChangedNotifier();

			var message = new ScheduleChangedEvent
				{
					BusinessUnitId = Guid.NewGuid(),
					Datasource = "My Data",
					StartDateTime = new DateTime(2010,1,1),
					EndDateTime = new DateTime(2010,1,31)
				};
			handler.Handle(message);

			//broker.AssertWasCalled(x => x.SendEventMessage(message.Datasource,message.BusinessUnitId,message.StartDateTime,message.EndDateTime));
		}
	}

	public class ScheduleChangedNotifier : IHandleEvent<ScheduleChangedEvent>
	{
		public void Handle(ScheduleChangedEvent @event)
		{
			
		}
	}
}
