using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAgentStatePersister : IAgentStatePersister
	{
		private readonly object _lock = new object();
		private IEnumerable<AgentStateFound> _data = Enumerable.Empty<AgentStateFound>();

		public AgentState ForPersonId(Guid personId)
		{
			return _data
				.FirstOrDefault(x => x.PersonId == personId);
		}

		public void Prepare(AgentStatePrepare model, DeadLockVictim deadLockVictim)
		{
			lock (_lock)
			{
				var copyFrom = _data.FirstOrDefault(x => x.PersonId == model.PersonId) ?? new AgentStateFound();
				_data = _data.Where(x => x.PersonId != model.PersonId).ToArray();
				model.ExternalLogons.ForEach(x =>
				{
					var adding = copyFrom.CopyBySerialization();
					adding.PersonId = model.PersonId;
					adding.BusinessUnitId = model.BusinessUnitId;
					adding.SiteId = model.SiteId;
					adding.TeamId = model.TeamId;
					adding.DataSourceId = x.DataSourceId;
					adding.UserCode = x.UserCode;
					_data = _data.Append(adding).ToArray();
				});
			}
		}

		public void Delete(Guid personId, DeadLockVictim deadLockVictim)
		{
			lock (_lock)
				_data = _data.Where(x => x.PersonId != personId).ToArray();
		}

		public void Update(AgentState model)
		{
			lock (_lock)
			{
				var updated = _data
					.Where(x => x.PersonId == model.PersonId)
					.Select(x =>
					{
						var updating = JsonConvert.DeserializeObject<AgentStateFound>(JsonConvert.SerializeObject(model));
						updating.DataSourceId = x.DataSourceId;
						updating.UserCode = x.UserCode;
						return updating;
					});

				_data = _data
					.Where(x => x.PersonId != model.PersonId)
					.Concat(updated)
					.ToArray();
			}
		}

		public virtual IEnumerable<ExternalLogonForCheck> FindForCheck()
		{
			lock (_lock)
				return _data
					.GroupBy(x => x.PersonId, (guid, states) => states.First())
					.Select(x => new ExternalLogonForCheck
					{
						PersonId = x.PersonId,
						DataSourceId = x.DataSourceId,
						UserCode = x.UserCode,
						LastCheck = x.ReceivedTime,
						LastTimeWindowCheckSum = x.TimeWindowCheckSum
					})
					.ToArray();
		}

		public virtual IEnumerable<ExternalLogon> FindForClosingSnapshot(DateTime snapshotId, string sourceId, string loggedOutState)
		{
			lock (_lock)
				return _data
					.Where(s => s.SourceId == sourceId && (s.BatchId < snapshotId || s.BatchId == null) && s.StateCode != loggedOutState)
					.GroupBy(x => x.PersonId, (guid, states) => states.First())
					.Select(x => new ExternalLogon
					{
						DataSourceId = x.DataSourceId,
						UserCode = x.UserCode,
						PersonId = x.PersonId
					})
					.ToArray();
		}

		public virtual IEnumerable<AgentStateFound> Find(IEnumerable<ExternalLogon> externalLogons, DeadLockVictim deadLockVictim)
		{
			lock (_lock)
				return _data
					.Where(x => externalLogons.Any(y => y.UserCode == x.UserCode && y.DataSourceId == x.DataSourceId))
					.ToArray();
		}

		public virtual IEnumerable<AgentState> Get(IEnumerable<Guid> personIds, DeadLockVictim deadLockVictim)
		{
			lock (_lock)
				return _data
					.Where(x => personIds.Contains(x.PersonId))
					.GroupBy(x => x.PersonId, (guid, states) => states.First())
					.ToArray();
		}

	}
}