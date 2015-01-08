using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public class AggregationState
	{
		private readonly ConcurrentDictionary<Guid, rtaAggregationData> _aggregationDatas = new ConcurrentDictionary<Guid, rtaAggregationData>();

		public bool Update(IAdherenceAggregatorInfo state)
		{
			var adherenceChanged = false;
			var actualAgentState = state.MakeActualAgentState();
			_aggregationDatas.AddOrUpdate(state.PersonId, guid =>
			{
				adherenceChanged = true;
				return new rtaAggregationData
				{
					ActualAgentState = actualAgentState,
					SiteId = state.SiteId,
					TeamId = state.TeamId
				};
			}
			, (guid, data) =>
			{
				adherenceChanged = !StateInfo.AdherenceFor(data.ActualAgentState).Equals(state.Adherence);
				data.ActualAgentState = actualAgentState;
				data.TeamId = state.TeamId;
				data.SiteId = state.SiteId;
				return data;
			});
			return adherenceChanged;
		}

		public int GetOutOfAdherenceForTeam(Guid teamId)
		{
			return _aggregationDatas
					.Where(k => k.Value.TeamId == teamId)
					.Count(x => StateInfo.AdherenceFor(x.Value.ActualAgentState).Equals(Domain.ApplicationLayer.Rta.Adherence.Out));
		}

		public int GetOutOfAdherenceForSite(Guid siteId)
		{
			return _aggregationDatas
				.Where(k => k.Value.SiteId == siteId)
				.Count(x => StateInfo.AdherenceFor(x.Value.ActualAgentState).Equals(Domain.ApplicationLayer.Rta.Adherence.Out));
		}

		public IEnumerable<IActualAgentState> GetActualAgentStateForTeam(Guid teamId)
		{
			return _aggregationDatas
				.Where(k => k.Value.TeamId == teamId)
				.Select(k => k.Value.ActualAgentState);
		}

		private class rtaAggregationData
		{
			public Guid SiteId { get; set; }
			public Guid TeamId { get; set; }
			public IActualAgentState ActualAgentState { get; set; }
		}

	}
}