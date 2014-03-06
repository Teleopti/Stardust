using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class AdherenceAggregator : IAfterSend
	{
		private readonly IMessageSender _messageSender;
		private readonly ITeamIdForPersonProvider _teamProvider;

		public AdherenceAggregator(IMessageSender messageSender, ITeamIdForPersonProvider teamProvider)
		{
			_messageSender = messageSender;
			_teamProvider = teamProvider;
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			var teamId = _teamProvider.GetTeamId(actualAgentState.PersonId);
			var teamAdherenceMessage = new TeamAdherenceMessage {TeamId = teamId};
			var notification = new Notification {BinaryData = JsonConvert.SerializeObject(teamAdherenceMessage)};
			_messageSender.SendNotification(notification);
		}
	}
}