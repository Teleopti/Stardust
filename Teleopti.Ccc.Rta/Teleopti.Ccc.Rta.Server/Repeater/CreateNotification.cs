using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.Server.Repeater
{
	public class CreateNotification : ICreateNotification
	{
		public Notification FromActualAgentState(IActualAgentState actualAgentState)
		{
			return NotificationFactory.CreateNotification(actualAgentState);
		}
	}
}