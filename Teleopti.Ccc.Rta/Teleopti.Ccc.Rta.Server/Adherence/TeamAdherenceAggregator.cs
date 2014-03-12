using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class TeamAdherenceAggregator
	{
		private readonly ITeamIdForPerson _teamProvider;
		private readonly Dictionary<Guid, AggregatedAdherence> _teamAdherences = new Dictionary<Guid, AggregatedAdherence>();

		public TeamAdherenceAggregator(ITeamIdForPerson teamProvider)
		{
			_teamProvider = teamProvider;
		}

		public AggregatedAdherence Aggregate(IActualAgentState actualAgentState)
		{
			if (_teamProvider == null) return null;
			
			var personId = actualAgentState.PersonId;
			var teamId = _teamProvider.GetTeamId(personId);

			AggregatedAdherence teamState;
			if (!_teamAdherences.TryGetValue(teamId, out teamState))
			{
				teamState = new AggregatedAdherence(teamId);
				_teamAdherences[teamId] = teamState;
			}
			var changed = teamState.TryUpdateAdherence(personId, actualAgentState.StaffingEffect);
			return changed ? teamState : null;
		}
	}
}