using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Server.Adherence;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.ServerTest.Adherence
{
	public class AdherenceAggregatorTest
	{
		[Test]
		public void ShouldSendMessageForTeam()
		{
			var agentState = new ActualAgentState();
			var notification = NotificationFactory.CreateNotification(new TeamAdherenceMessage());
			var broker = MockRepository.GenerateMock<IMessageSender>();
			var target = new AdherenceAggregator(broker);

			target.Invoke(agentState);

			broker.AssertWasCalled(x => x.SendNotification(notification),(a)=> a.IgnoreArguments());
		}
	}
}
