using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.Persisters.NewStuff
{
	public interface IMessageQueueRemoval
	{
		void Remove(IEventMessage eventMessage);
		void Remove(Guid id);
	}
}