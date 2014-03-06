using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class AdherenceAggregator : IAfterSend
	{
		private readonly IMessageSender _messageSender;

		public AdherenceAggregator(IMessageSender messageSender)
		{
			_messageSender = messageSender;
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			_messageSender.SendNotification(new Notification());
		}
	}
}