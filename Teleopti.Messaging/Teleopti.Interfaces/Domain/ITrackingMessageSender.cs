using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Interfaces.Domain
{
	public interface ITrackingMessageSender
	{
		void SendTrackingMessage(ILogOnInfo originatingEvent, TrackingMessage message);
	}
}