using System;

namespace Teleopti.Interfaces.MessageBroker.Events
{
	public interface ITrackingMessageSender
	{
		void SendTrackingMessage(Guid initiatorId, Guid businessUnitId, TrackingMessage message);
	}
}