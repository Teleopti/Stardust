using System;
using Newtonsoft.Json;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;
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

		public Notification CreateNotification(PersonOrganizationData personOrganizationData, IActualAgentState actualAgentState)
		{
			var numberOfOutOfAdherence = _aggregationState.GetOutOfAdherenceForTeam(personOrganizationData.TeamId);
			return createTeamNotification(numberOfOutOfAdherence, actualAgentState.BusinessUnit, personOrganizationData.TeamId, personOrganizationData.SiteId);
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