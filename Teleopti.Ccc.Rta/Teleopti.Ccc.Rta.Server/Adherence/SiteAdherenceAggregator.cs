using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class SiteAdherenceAggregator
	{
		private readonly ISiteIdForPerson _siteIdForPerson;
		private readonly Dictionary<Guid, SiteAdherence> _siteAdherences = new Dictionary<Guid, SiteAdherence>();

		public SiteAdherenceAggregator(ISiteIdForPerson siteIdForPerson)
		{
			_siteIdForPerson = siteIdForPerson;
		}

		public SiteAdherence Aggregate(IActualAgentState actualAgentState)
		{
			if (_siteIdForPerson == null) return null;
			var personId = actualAgentState.PersonId;
			var siteId = _siteIdForPerson.GetSiteId(personId);

			if (!_siteAdherences.ContainsKey(siteId))
				_siteAdherences[siteId] = new SiteAdherence(siteId);

			var teamState = _siteAdherences[siteId];
			var changed = teamState.TryUpdateAdherence(personId, actualAgentState.StaffingEffect);
			return !changed ? null : _siteAdherences[siteId];
		}
	}
}