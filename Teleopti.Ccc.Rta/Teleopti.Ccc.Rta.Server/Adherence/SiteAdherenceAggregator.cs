using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class SiteAdherenceAggregator
	{
		private readonly IOrganizationForPerson _organizationForPerson;
		private readonly Dictionary<Guid, AggregatedValues> _siteAdherences = new Dictionary<Guid, AggregatedValues>();

		public SiteAdherenceAggregator(IOrganizationForPerson organizationForPerson)
		{
			_organizationForPerson = organizationForPerson;
		}

		public AggregatedValues Aggregate(IActualAgentState actualAgentState)
		{
			if (_organizationForPerson == null) return null;

			var personId = actualAgentState.PersonId;
			var siteId = _organizationForPerson.GetOrganization(personId).SiteId;

			AggregatedValues siteState;
			if (!_siteAdherences.TryGetValue(siteId, out siteState))
			{
				siteState = new AggregatedValues(siteId);
				_siteAdherences[siteId] = siteState;
			}

			var changed = siteState.TryUpdateAdherence(personId, actualAgentState.StaffingEffect);
			return changed ? _siteAdherences[siteId] : null;
		}
	}
}