using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public interface IAdherenceAggregator
	{
		void Aggregate(IActualAgentState actualAgentState);
		void Initialize(IActualAgentState actualAgentState);
	}

	public class AdherenceAggregator : IAdherenceAggregator
	{
		private readonly IMessageSender _messageSender;
		private readonly TeamAdherenceAggregator _teamAdherenceAggregator;
		private readonly SiteAdherenceAggregator _siteAdherenceAggregator;
		private readonly AgentAdherenceAggregator _agentAdherenceAggregator;
		private readonly IOrganizationForPerson _organizationForPerson;
		private readonly AggregationState _aggregationState;

		public AdherenceAggregator(IMessageSender messageSender, IOrganizationForPerson organizationForPerson)
		{
			_messageSender = messageSender;
			_organizationForPerson = organizationForPerson;
			_aggregationState = new AggregationState();
			_teamAdherenceAggregator = new TeamAdherenceAggregator(_aggregationState);
			_siteAdherenceAggregator = new SiteAdherenceAggregator(_aggregationState);
			_agentAdherenceAggregator = new AgentAdherenceAggregator(_aggregationState);
		}

		public void Aggregate(IActualAgentState actualAgentState)
		{
			aggregate(actualAgentState, true);
		}

		public void Initialize(IActualAgentState actualAgentState)
		{
			aggregate(actualAgentState, false);
		}

		private void aggregate(IActualAgentState actualAgentState, bool sendMessages)
		{
			var personOrganizationData = _organizationForPerson.GetOrganization(actualAgentState.PersonId);

			if (personOrganizationData == null)
				return;

			var adherenceChanged = _aggregationState.Update(personOrganizationData, actualAgentState);

			if (!sendMessages)
				return;

			var agentsAdherence = _agentAdherenceAggregator.CreateNotification(personOrganizationData, actualAgentState);
			if (agentsAdherence != null)
				_messageSender.Send(agentsAdherence);

			if (!adherenceChanged)
				return;
			
			var siteAdherence = _siteAdherenceAggregator.CreateNotification(personOrganizationData, actualAgentState);
			if (siteAdherence != null)
				_messageSender.Send(siteAdherence);

			var teamAdherence = _teamAdherenceAggregator.CreateNotification(personOrganizationData, actualAgentState);
			if (teamAdherence != null)
				_messageSender.Send(teamAdherence);

		}

	}
}