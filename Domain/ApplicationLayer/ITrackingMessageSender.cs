using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface ITrackingMessageSender
	{
		void SendTrackingMessage(IEvent originatingEvent, TrackingMessage message);
	}
}