using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class RtaAggregationState
	{
		private readonly IOrganizationForPerson _organizationForPerson;
		private readonly ConcurrentDictionary<Guid, RtaAggregationData> _aggregationDatas;

		public RtaAggregationState(IOrganizationForPerson organizationForPerson)
		{
			_organizationForPerson = organizationForPerson;
			_aggregationDatas = new ConcurrentDictionary<Guid, RtaAggregationData>();
		}

		public PersonOrganizationData Update(Guid personId, IActualAgentState actualAgentState)
		{
			var personOrganizationData = _organizationForPerson.GetOrganization(personId);

			_aggregationDatas.AddOrUpdate(personId, guid => new RtaAggregationData
			{
				ActualAgentState = actualAgentState,
				OrganizationData = personOrganizationData
			}, (guid, data) =>
			{
				data.ActualAgentState = actualAgentState;
				data.OrganizationData = personOrganizationData;
				return data;
			});

			return personOrganizationData;
		}

		public IEnumerable<IActualAgentState> GetActualAgentStateForTeam(Guid teamId)
		{
			return _aggregationDatas.Where(k => k.Value.OrganizationData.TeamId == teamId).Select(x => x.Value.ActualAgentState);
		}

		public IEnumerable<IActualAgentState> GetActualAgentStateForSite(Guid siteId)
		{
			return _aggregationDatas.Where(k => k.Value.OrganizationData.SiteId == siteId).Select(x => x.Value.ActualAgentState);
		}
	}
}