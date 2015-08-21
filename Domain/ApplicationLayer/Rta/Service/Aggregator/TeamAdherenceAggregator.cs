﻿using System;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator
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

		public Interfaces.MessageBroker.Message CreateNotification(IAdherenceAggregatorInfo state)
		{
			var numberOfOutOfAdherence = _aggregationState.GetOutOfAdherenceForTeam(state.Person.TeamId);
			return createTeamNotification(numberOfOutOfAdherence, 
				state.Person.BusinessUnitId, 
				state.Person.TeamId, 
				state.Person.SiteId);
		}

		private Interfaces.MessageBroker.Message createTeamNotification(int numberOfOutOfAdherence, Guid businessUnitId, Guid teamId, Guid siteId)
		{
			var teamAdherenceMessage = new TeamAdherenceMessage
			{
				OutOfAdherence = numberOfOutOfAdherence
			};

			return new Interfaces.MessageBroker.Message
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