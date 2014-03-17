using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class TeamAdherenceAggregator
	{
		private readonly ITeamIdForPerson _teamProvider;
		private readonly Dictionary<Guid, AggregatedValues> _teamAdherences = new Dictionary<Guid, AggregatedValues>();

		public TeamAdherenceAggregator(ITeamIdForPerson teamProvider)
		{
			_teamProvider = teamProvider;
		}

		public AggregatedValues Aggregate(IActualAgentState actualAgentState)
		{
			if (_teamProvider == null) return null;
			
			var personId = actualAgentState.PersonId;
			var teamId = _teamProvider.GetTeamId(personId);

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