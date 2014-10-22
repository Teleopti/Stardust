﻿using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public interface IAdherenceAggregator : IActualAgentStateHasBeenSent
	{
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

		public void Invoke(IActualAgentState actualAgentState)
		{
			var personOrganizationData = _organizationForPerson.GetOrganization(actualAgentState.PersonId);

			if (personOrganizationData == null)
				return;

			_aggregationState.Update(personOrganizationData, actualAgentState);

			var siteAdherence = _siteAdherenceAggregator.CreateNotification(personOrganizationData, actualAgentState);
			if (siteAdherence != null)
				_messageSender.Send(siteAdherence);

			var teamAdherence = _teamAdherenceAggregator.CreateNotification(personOrganizationData, actualAgentState);
			if (teamAdherence != null)
				_messageSender.Send(teamAdherence);

			var agentsAdherence = _agentAdherenceAggregator.CreateNotification(personOrganizationData, actualAgentState);
			if (agentsAdherence != null)
				_messageSender.Send(agentsAdherence);
		}
	}
}