using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public class AggregationState
	{
		private readonly ConcurrentDictionary<Guid, rtaAggregationData> _aggregationDatas = new ConcurrentDictionary<Guid, rtaAggregationData>();

		public bool Update(IAdherenceAggregatorInfo state)
		{
			var adherenceChanged = false;
			_aggregationDatas.AddOrUpdate(state.PersonOrganizationData.PersonId, guid =>
			{
				adherenceChanged = true;
				return new rtaAggregationData
				{
					ActualAgentState = state.NewState,
					OrganizationData = state.PersonOrganizationData
				};
			}
			, (guid, data) =>
			{
				adherenceChanged = !StateInfo.AdherenceFor(data.ActualAgentState).Equals(state.InAdherence);
				data.ActualAgentState = state.NewState;
				data.OrganizationData = state.PersonOrganizationData;
				return data;
			});
			return adherenceChanged;
		}

		public int GetOutOfAdherenceForTeam(Guid teamId)
		{
			return _aggregationDatas
					.Where(k => k.Value.OrganizationData.TeamId == teamId)
					.Count(x => !StateInfo.AdherenceFor(x.Value.ActualAgentState));
		}

		public int GetOutOfAdherenceForSite(Guid siteId)
		{
			return _aggregationDatas
				.Where(k => k.Value.OrganizationData.SiteId == siteId)
				.Count(x => !StateInfo.AdherenceFor(x.Value.ActualAgentState));
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