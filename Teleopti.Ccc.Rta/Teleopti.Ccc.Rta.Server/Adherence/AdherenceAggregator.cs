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
			var siteAdherence = _siteAdherenceAggregator.CreateNotification(actualAgentState);
			if (siteAdherence != null)
				_messageSender.SendNotification(siteAdherence);

			var teamAdherence = _teamAdherenceAggregator.CreateNotification(actualAgentState);
			if (teamAdherence != null)
				_messageSender.SendNotification(teamAdherence);
		}
	}
}