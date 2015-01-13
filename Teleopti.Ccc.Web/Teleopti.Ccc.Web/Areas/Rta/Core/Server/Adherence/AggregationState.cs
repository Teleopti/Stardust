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
					AgentStateReadModel = actualAgentState,
					SiteId = state.SiteId,
					TeamId = state.TeamId
				};
			}
			, (guid, data) =>
			{
				adherenceChanged = !StateInfo.AdherenceFor(data.AgentStateReadModel).Equals(state.Adherence);
				data.AgentStateReadModel = actualAgentState;
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
					.Count(x => StateInfo.AdherenceFor(x.Value.AgentStateReadModel).Equals(Domain.ApplicationLayer.Rta.Adherence.Out));
		}

		public int GetOutOfAdherenceForSite(Guid siteId)
		{
			return _aggregationDatas
				.Where(k => k.Value.SiteId == siteId)
				.Count(x => StateInfo.AdherenceFor(x.Value.AgentStateReadModel).Equals(Domain.ApplicationLayer.Rta.Adherence.Out));
		}

		public IEnumerable<AgentStateReadModel> GetActualAgentStateForTeam(Guid teamId)
		{
			return _aggregationDatas
				.Where(k => k.Value.TeamId == teamId)
				.Select(k => k.Value.AgentStateReadModel);
		}

		private class rtaAggregationData
		{
			public Guid SiteId { get; set; }
			public Guid TeamId { get; set; }
			public AgentStateReadModel AgentStateReadModel { get; set; }
		}

	}
}