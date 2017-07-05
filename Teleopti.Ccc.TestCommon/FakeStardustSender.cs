using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeStardustSender : IStardustSender
	{
		public IList<IEvent> SentMessages = new List<IEvent>();

		public Guid Send(IEvent @event)
		{
			SentMessages.Add(@event);
			return Guid.NewGuid();
		}
	}
}