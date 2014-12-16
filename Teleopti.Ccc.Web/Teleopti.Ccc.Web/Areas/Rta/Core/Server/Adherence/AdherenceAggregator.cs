using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public class AdherenceAggregator
	{
		private readonly IMessageSender _messageSender;
		private readonly TeamAdherenceAggregator _teamAdherenceAggregator;
		private readonly SiteAdherenceAggregator _siteAdherenceAggregator;
		private readonly AgentAdherenceAggregator _agentAdherenceAggregator;
		private readonly AggregationState _aggregationState;

		public AdherenceAggregator(IMessageSender messageSender)
		{
			_messageSender = messageSender;
			_aggregationState = new AggregationState();
			_teamAdherenceAggregator = new TeamAdherenceAggregator(_aggregationState);
			_siteAdherenceAggregator = new SiteAdherenceAggregator(_aggregationState);
			_agentAdherenceAggregator = new AgentAdherenceAggregator(_aggregationState);
		}

		public void Aggregate(IAdherenceAggregatorInfo state)
		{
			aggregate(state, true);
		}

		public void Initialize(IAdherenceAggregatorInfo state)
		{
			aggregate(state, false);
		}

		private void aggregate(IAdherenceAggregatorInfo state, bool sendMessages)
		{
			var adherenceChanged = _aggregationState.Update(state);

			if (!sendMessages)
				return;

			var agentsAdherences = _agentAdherenceAggregator.CreateNotification(state);
			if (agentsAdherences != null)
				agentsAdherences.ForEach(_messageSender.Send);

			if (!adherenceChanged)
				return;

			var siteAdherence = _siteAdherenceAggregator.CreateNotification(state);
			if (siteAdherence != null)
				_messageSender.Send(siteAdherence);

			var teamAdherence = _teamAdherenceAggregator.CreateNotification(state);
			if (teamAdherence != null)
				_messageSender.Send(teamAdherence);

		}

	}

}