using System;

namespace Teleopti.Interfaces.MessageBroker.Events
{
	public interface IEventTracker
	{
		void SendTrackingMessage(Guid personId, Guid businessUnitId, Guid trackId);
	}
}