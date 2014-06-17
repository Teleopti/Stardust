using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class RtaAggregationState
	{
		private readonly IOrganizationForPerson _organizationForPerson;
		private readonly IList<RtaAggregationData> snarr = new List<RtaAggregationData>();

		public RtaAggregationState(IOrganizationForPerson organizationForPerson)
		{
			_organizationForPerson = organizationForPerson;
		}

		public PersonOrganizationData Update(Guid personId, IActualAgentState actualAgentState)
		{
			var personOrganizationData = _organizationForPerson.GetOrganization(personId);
			var snarret = snarr.SingleOrDefault(x => x.OrganizationData.PersonId == personId);
			if (snarret == null)
			{
				snarr.Add(new RtaAggregationData { ActualAgentState = actualAgentState, OrganizationData = personOrganizationData });
			}
			else
			{
				snarret.OrganizationData = personOrganizationData;
				snarret.ActualAgentState = actualAgentState;
			}
			return personOrganizationData;
		}

		public IEnumerable<IActualAgentState> GetActualAgentStateForTeam(Guid teamId)
		{
			return snarr.Where(k => k.OrganizationData.TeamId == teamId).Select(x => x.ActualAgentState);
		}

		public IEnumerable<IActualAgentState> GetActualAgentStateForSite(Guid siteId)
		{
			return snarr.Where(k => k.OrganizationData.SiteId == siteId).Select(x => x.ActualAgentState);
		}
	}
}