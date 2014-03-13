using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class SiteAdherenceAggregator
	{
		private readonly ISiteIdForPerson _siteIdForPerson;
		private readonly Dictionary<Guid, AggregatedAdherence> _siteAdherences = new Dictionary<Guid, AggregatedAdherence>();

		public SiteAdherenceAggregator(ISiteIdForPerson siteIdForPerson)
		{
			_siteIdForPerson = siteIdForPerson;
		}

		public AggregatedAdherence Aggregate(IActualAgentState actualAgentState)
		{
			if (_siteIdForPerson == null) return null;

			var personId = actualAgentState.PersonId;
			var siteId = _siteIdForPerson.GetSiteId(personId);

			AggregatedAdherence siteState;
			if (!_siteAdherences.TryGetValue(siteId, out siteState))
			{
				siteState = new AggregatedAdherence(siteId);
				_siteAdherences[siteId] = siteState;
			}

			var changed = siteState.TryUpdateAdherence(personId, actualAgentState.StaffingEffect);
			return changed ? _siteAdherences[siteId] : null;
		}
	}
}