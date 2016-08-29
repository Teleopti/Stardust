using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAgentStatePersister : IAgentStatePersister
	{
		private readonly object _lock = new object();
		private readonly List<data> _data = new List<data>();

		private class data
		{
			public AgentStatePrepare added;
			public AgentState state;
		}

		public void Prepare(AgentStatePrepare model)
		{
			lock(_lock)
				_data.Add(new data
				{
					added = model,
					state = new AgentState
					{
						PersonId = model.PersonId,
						BusinessUnitId = model.BusinessUnitId,
						SiteId = model.SiteId,
						TeamId = model.TeamId
					}
				});
		}

		public void Delete(Guid personId)
		{
			lock (_lock)
			{
				var existing = _data.Where(x => x.added.PersonId == personId).ToArray();
				existing.ForEach(x => _data.Remove(x));
			}
		}

		public void Update(AgentState model)
		{
			lock (_lock)
				_data
					.Where(x => x.added.PersonId == model.PersonId)
					.ForEach(x => x.state = model);
		}

		public IEnumerable<AgentState> Get(int dataSourceId, string userCode)
		{
			lock (_lock)
				return _data
					.Where(x => x.added.ExternalLogons.Any(y => y.DataSourceId == dataSourceId && y.UserCode == userCode))
					.Select(x => x.state)
					.ToArray();
		}

		public AgentState Get(Guid personId)
		{
			lock (_lock)
				return _data
					.Select(x => x.state)
					.FirstOrDefault(x => x.PersonId == personId);
		}

		public IEnumerable<AgentState> GetNotInSnapshot(DateTime snapshotId, string sourceId)
		{
			lock (_lock)
				return _data
					.Select(x => x.state)
					.Where(s => s.SourceId == sourceId && (s.BatchId < snapshotId || s.BatchId == null))
					.GroupBy(x => x.PersonId, (guid, states) => states.First())
					.ToArray();
		}

		public IEnumerable<AgentState> Get(IEnumerable<Guid> personIds)
		{
			lock (_lock)
				return _data
					.Where(x => personIds.Contains(x.added.PersonId))
					.Select(x => x.state)
					.GroupBy(x => x.PersonId, (guid, states) => states.First())
					.ToArray();
		}

		public IEnumerable<AgentState> GetAll()
		{
			lock (_lock)
				return _data
					.Select(x => x.state)
					.GroupBy(x => x.PersonId, (guid, states) => states.First())
					.ToArray();
		}

	}
}