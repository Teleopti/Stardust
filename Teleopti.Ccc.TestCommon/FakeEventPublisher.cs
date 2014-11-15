using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeEventPublisher : IEventPublisher
	{
		public IList<IEvent> PublishedEvents = new List<IEvent>();

		public void Publish(IEvent @event)
		{
			PublishedEvents.Add(@event);
		}
	}

}