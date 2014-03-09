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
		private readonly TeamAdherenceAggregator _teamAdherenceAggregator;
		private readonly SiteAdherenceAggregator _siteAdherenceAggregator;

		public AdherenceAggregator(IMessageSender messageSender, ITeamIdForPerson teamProvider, ISiteIdForPerson siteProvider)
		{
			_messageSender = messageSender;
			_teamAdherenceAggregator = new TeamAdherenceAggregator(teamProvider);
			_siteAdherenceAggregator = new SiteAdherenceAggregator(siteProvider);
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			var siteAdherence = _siteAdherenceAggregator.Aggregate(actualAgentState);
			if (siteAdherence != null)
				_messageSender.SendNotification(createSiteNotification(siteAdherence));

			var teamAdherence = _teamAdherenceAggregator.Aggregate(actualAgentState);
			if (teamAdherence != null)
				_messageSender.SendNotification(createTeamNotification(teamAdherence));
		}

		private static Notification createTeamNotification(AggregatedAdherence aggregatedAdherence)
		{
			var teamAdherenceMessage = new TeamAdherenceMessage
				{
					TeamId = aggregatedAdherence.Key,
					OutOfAdherence = aggregatedAdherence.NumberOutOfAdherence()
				};
			var notification = new Notification {BinaryData = JsonConvert.SerializeObject(teamAdherenceMessage)};
			return notification;
		}

		private static Notification createSiteNotification(AggregatedAdherence aggregatedAdherence)
		{
			var siteAdherenceMessage = new SiteAdherenceMessage
			{
				SiteId = aggregatedAdherence.Key,
				OutOfAdherence = aggregatedAdherence.NumberOutOfAdherence()
			};
			var notification = new Notification { BinaryData = JsonConvert.SerializeObject(siteAdherenceMessage) };
			return notification;
		}
	}
}