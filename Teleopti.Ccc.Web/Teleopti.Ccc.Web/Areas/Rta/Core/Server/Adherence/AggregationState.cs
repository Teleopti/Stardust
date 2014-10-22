using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public class AggregationState
	{
		private readonly ConcurrentDictionary<Guid, rtaAggregationData> _aggregationDatas = new ConcurrentDictionary<Guid, rtaAggregationData>();

		public bool Update(PersonOrganizationData personOrganizationData, IActualAgentState actualAgentState)
		{
			var adherenceChanged = false;
			_aggregationDatas.AddOrUpdate(personOrganizationData.PersonId, guid =>
			{
				adherenceChanged = true;
				return new rtaAggregationData
				{
					ActualAgentState = actualAgentState,
					OrganizationData = personOrganizationData
				};
			}
			, (guid, data) =>
			{
				adherenceChanged = !data.ActualAgentState.StaffingEffect.Equals(actualAgentState.StaffingEffect);
				data.ActualAgentState = actualAgentState;
				data.OrganizationData = personOrganizationData;
				return data;
			});
			return adherenceChanged;
		}

		public int GetOutOfAdherenceForTeam(Guid teamId)
		{
			return _aggregationDatas
					.Where(k => k.Value.OrganizationData.TeamId == teamId)
					.Count(x => Math.Abs(x.Value.ActualAgentState.StaffingEffect) > 0.01);
		}

		public int GetOutOfAdherenceForSite(Guid siteId)
		{
			return _aggregationDatas
				.Where(k => k.Value.OrganizationData.SiteId == siteId)
				.Count(x => Math.Abs(x.Value.ActualAgentState.StaffingEffect) > 0.01);
		}

		public IEnumerable<IActualAgentState> GetActualAgentStateForTeam(Guid teamId)
		{
			return _aggregationDatas
				.Where(k => k.Value.OrganizationData.TeamId == teamId)
				.Select(k => k.Value.ActualAgentState);
		}

		private class rtaAggregationData
		{
			public PersonOrganizationData OrganizationData { get; set; }
			public IActualAgentState ActualAgentState { get; set; }
		}

	}
}