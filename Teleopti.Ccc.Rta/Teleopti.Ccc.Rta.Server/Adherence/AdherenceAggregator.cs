using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class AdherenceAggregator : IActualAgentStateHasBeenSent
	{
		private readonly IMessageSender _messageSender;
		private readonly TeamAdherenceAggregator _teamAdherenceAggregator;
		private readonly SiteAdherenceAggregator _siteAdherenceAggregator;
		private readonly AgentAdherenceAggregator _agentAdherenceAggregator;

		public AdherenceAggregator(IMessageSender messageSender, IOrganizationForPerson organizationForPerson)
		{
			var theUglyMan = new RtaAggregationStateProvider(organizationForPerson);
			_messageSender = messageSender;
			_teamAdherenceAggregator = new TeamAdherenceAggregator(theUglyMan);
			_siteAdherenceAggregator = new SiteAdherenceAggregator(theUglyMan);
			_agentAdherenceAggregator = new AgentAdherenceAggregator(organizationForPerson);
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			var siteAdherence = _siteAdherenceAggregator.CreateNotification(actualAgentState);
			if (siteAdherence != null)
				_messageSender.SendNotification(siteAdherence);

			var teamAdherence = _teamAdherenceAggregator.CreateNotification(actualAgentState);
			if (teamAdherence != null)
				_messageSender.SendNotification(teamAdherence);

			var agentsAdherence = _agentAdherenceAggregator.CreateNotification(actualAgentState);
			if (agentsAdherence != null)
				_messageSender.SendNotification(agentsAdherence);
		}
	}
}