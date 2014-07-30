using System;

namespace Teleopti.Interfaces.MessageBroker.Events
{
	public interface IEventTracker
	{
		void SendTrackingMessage(Guid initiatorId, Guid businessUnitId, Guid trackId);
	}
}