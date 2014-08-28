using System.Runtime.Remoting.Messaging;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class AdherenceAggregator : IActualAgentStateHasBeenSent
	{
		private readonly IMessageSender _signalRClient;
		private readonly TeamAdherenceAggregator _teamAdherenceAggregator;
		private readonly SiteAdherenceAggregator _siteAdherenceAggregator;
		private readonly AgentAdherenceAggregator _agentAdherenceAggregator;
		private readonly IOrganizationForPerson _organizationForPerson;

		public AdherenceAggregator(IMessageSender signalRClient, IOrganizationForPerson organizationForPerson)
		{
			_signalRClient = signalRClient;
			_organizationForPerson = organizationForPerson;
			var stateProvider = new RtaAggregationStateProvider(organizationForPerson);
			_teamAdherenceAggregator = new TeamAdherenceAggregator(stateProvider);
			_siteAdherenceAggregator = new SiteAdherenceAggregator(stateProvider);
			_agentAdherenceAggregator = new AgentAdherenceAggregator(stateProvider);
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			if (_organizationForPerson.GetOrganization(actualAgentState.PersonId) == null)
				return;

			var siteAdherence = _siteAdherenceAggregator.CreateNotification(actualAgentState);
			if (siteAdherence != null)
				_signalRClient.SendNotification(siteAdherence);

			var teamAdherence = _teamAdherenceAggregator.CreateNotification(actualAgentState);
			if (teamAdherence != null)
				_signalRClient.SendNotification(teamAdherence);

			var agentsAdherence = _agentAdherenceAggregator.CreateNotification(actualAgentState);
			if (agentsAdherence != null)
				_signalRClient.SendNotification(agentsAdherence);
		}
	}
}