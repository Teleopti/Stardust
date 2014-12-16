﻿using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public class TeamAdherenceAggregator
	{
		private readonly AggregationState _aggregationState;

		public TeamAdherenceAggregator(AggregationState aggregationState)
		{
			_aggregationState = aggregationState;
		}

		public Notification CreateNotification(IAdherenceAggregatorInfo state)
		{
			var numberOfOutOfAdherence = _aggregationState.GetOutOfAdherenceForTeam(state.PersonOrganizationData.TeamId);
			return createTeamNotification(numberOfOutOfAdherence, 
				state.NewState.BusinessUnitId, 
				state.PersonOrganizationData.TeamId, 
				state.PersonOrganizationData.SiteId);
		}

		private static Notification createTeamNotification(int numberOfOutOfAdherence, Guid businessUnitId, Guid teamId, Guid siteId)
		{
			var teamAdherenceMessage = new TeamAdherenceMessage
			{
				OutOfAdherence = numberOfOutOfAdherence
			};

			return new Notification
			{
				BinaryData = JsonConvert.SerializeObject(teamAdherenceMessage),
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = "TeamAdherenceMessage",
				DomainId = teamId.ToString(),
				DomainReferenceId = siteId.ToString()
			};
		}
	}
}