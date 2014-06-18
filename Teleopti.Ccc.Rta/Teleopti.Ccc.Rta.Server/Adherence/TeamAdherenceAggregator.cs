using System;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class TeamAdherenceAggregator
	{
		private readonly RtaAggregationStateProvider _stateProvider;

		public TeamAdherenceAggregator(RtaAggregationStateProvider stateProvider)
		{
			_stateProvider = stateProvider;
		}

		public Notification CreateNotification(IActualAgentState actualAgentState)
		{
			var aggregationState = _stateProvider.GetState();
			var personOrganizationData = aggregationState.Update(actualAgentState.PersonId, actualAgentState);
			var numberOfOutOfAdherence = aggregationState.GetActualAgentStateForTeam(personOrganizationData.TeamId).Count(x => Math.Abs(x.StaffingEffect) > 0.01);

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