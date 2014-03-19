using System;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class AdherenceAggregator : IActualAgentStateHasBeenSent
	{
		private readonly IMessageSender _messageSender;
		private readonly TeamAdherenceAggregator _teamAdherenceAggregator;
		private readonly SiteAdherenceAggregator _siteAdherenceAggregator;

		public AdherenceAggregator(IMessageSender messageSender, IOrganizationForPerson organizationForPerson)
		{
			_messageSender = messageSender;
			_teamAdherenceAggregator = new TeamAdherenceAggregator(organizationForPerson);
			_siteAdherenceAggregator = new SiteAdherenceAggregator(organizationForPerson);
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			var siteAdherence = _siteAdherenceAggregator.Aggregate(actualAgentState);
			if (siteAdherence != null)
				_messageSender.SendNotification(createSiteNotification(siteAdherence, actualAgentState.BusinessUnit));

			var teamAdherence = _teamAdherenceAggregator.Aggregate(actualAgentState);
			if (teamAdherence != null)
				_messageSender.SendNotification(createTeamNotification(teamAdherence, actualAgentState.BusinessUnit));
		}

		private static Notification createTeamNotification(AggregatedValues aggregatedValues, Guid businessUnitId)
		{
			var teamAdherenceMessage = new TeamAdherenceMessage
			{
				OutOfAdherence = aggregatedValues.NumberOutOfAdherence()
			};
			return createNotification("TeamAdherenceMessage", teamAdherenceMessage, businessUnitId, aggregatedValues);
		}

		private static Notification createSiteNotification(AggregatedValues aggregatedValues, Guid businessUnitId)
		{
			var siteAdherenceMessage = new SiteAdherenceMessage
			{
				OutOfAdherence = aggregatedValues.NumberOutOfAdherence()
			};
			return createNotification("SiteAdherenceMessage", siteAdherenceMessage, businessUnitId, aggregatedValues);
		}

		private static Notification createNotification(string domainType, object message, Guid businessUnitId, AggregatedValues aggregatedValues)
		{
			return new Notification
			{
				BinaryData = JsonConvert.SerializeObject(message),
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = domainType,
				DomainId = aggregatedValues.Key.ToString()
			};
		}
	}
}