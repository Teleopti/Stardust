using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.Domain
{
	public interface ITrackingMessageSender
	{
		void SendTrackingMessage(Guid initiatorId, Guid businessUnitId, TrackingMessage message);
	}
}