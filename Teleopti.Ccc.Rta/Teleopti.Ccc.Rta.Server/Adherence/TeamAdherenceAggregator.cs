using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

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

		public AggregatedValues Aggregate(IActualAgentState actualAgentState)
		{
			if (_organizationForPerson == null) return null;

			var personId = actualAgentState.PersonId;
			var teamId = _organizationForPerson.GetOrganization(personId).TeamId;

			AggregatedValues teamState;
			if (!_teamAdherences.TryGetValue(teamId, out teamState))
			{
				teamState = new AggregatedValues(teamId);
				_teamAdherences[teamId] = teamState;
			}
			var changed = teamState.TryUpdateAdherence(personId, actualAgentState.StaffingEffect);
			return changed ? teamState : null;
		}
	}
}