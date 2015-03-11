using System;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aggregator
{
	public class TeamAdherenceAggregator
	{
		private readonly AggregationState _aggregationState;
		private readonly IJsonSerializer _jsonSerializer;

		public TeamAdherenceAggregator(AggregationState aggregationState, IJsonSerializer jsonSerializer)
		{
			_aggregationState = aggregationState;
			_jsonSerializer = jsonSerializer;
		}

		public Interfaces.MessageBroker.Notification CreateNotification(IAdherenceAggregatorInfo state)
		{
			var numberOfOutOfAdherence = _aggregationState.GetOutOfAdherenceForTeam(state.TeamId);
			return createTeamNotification(numberOfOutOfAdherence, 
				state.BusinessUnitId, 
				state.TeamId, 
				state.SiteId);
		}

		private Interfaces.MessageBroker.Notification createTeamNotification(int numberOfOutOfAdherence, Guid businessUnitId, Guid teamId, Guid siteId)
		{
			var teamAdherenceMessage = new TeamAdherenceMessage
			{
				OutOfAdherence = numberOfOutOfAdherence
			};

			return new Interfaces.MessageBroker.Notification
			{
				BinaryData = _jsonSerializer.SerializeObject(teamAdherenceMessage),
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = "TeamAdherenceMessage",
				DomainId = teamId.ToString(),
				DomainReferenceId = siteId.ToString()
			};
		}
	}
}