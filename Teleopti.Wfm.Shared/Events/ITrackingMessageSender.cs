using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface ITrackingMessageSender
	{
		void SendTrackingMessage(IEvent originatingEvent, TrackingMessage message);
	}
}