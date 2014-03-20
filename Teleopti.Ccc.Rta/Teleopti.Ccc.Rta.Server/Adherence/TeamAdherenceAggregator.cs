using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class TeamAdherenceAggregator
	{
		private readonly IOrganizationForPerson _organizationForPerson;
		private readonly Dictionary<Guid, AggregatedValues> _teamAdherences = new Dictionary<Guid, AggregatedValues>();

		public TeamAdherenceAggregator(IOrganizationForPerson organizationForPerson)
		{
			_organizationForPerson = organizationForPerson;
		}

		public Notification CreateNotification(IActualAgentState actualAgentState)
		{
			if (_organizationForPerson == null) return null;

			var personId = actualAgentState.PersonId;
			var personOrganizationData = _organizationForPerson.GetOrganization(personId);
			var teamId = personOrganizationData.TeamId;

			AggregatedValues teamState;
			if (!_teamAdherences.TryGetValue(teamId, out teamState))
			{
				teamState = new AggregatedValues(teamId);
				_teamAdherences[teamId] = teamState;
			}
			var changed = teamState.TryUpdateAdherence(personId, actualAgentState.StaffingEffect);
			return changed
				? createTeamNotification(_teamAdherences[teamId], actualAgentState.BusinessUnit, personOrganizationData.SiteId)
				: null;
		}

		private static Notification createTeamNotification(AggregatedValues aggregatedValues, Guid businessUnitId, Guid siteId)
		{
			var teamAdherenceMessage = new TeamAdherenceMessage
			{
				OutOfAdherence = aggregatedValues.NumberOutOfAdherence()
			};

			return new Notification
			{
				BinaryData = JsonConvert.SerializeObject(teamAdherenceMessage),
				BusinessUnitId = businessUnitId.ToString(),
				DomainType = "TeamAdherenceMessage",
				DomainId = aggregatedValues.Key.ToString(),
				DomainReferenceId = siteId.ToString()
			};
		}
	}
}