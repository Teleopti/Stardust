using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Wfm.Adherence;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAgentStatePersister : IAgentStatePersister
	{
		private readonly IKeyValueStorePersister _keyValueStore;
		private readonly object _lock = new object();
		private IEnumerable<AgentState> _data = Enumerable.Empty<AgentState>();

		public FakeAgentStatePersister(IKeyValueStorePersister keyValueStore)
		{
			_keyValueStore = keyValueStore;
		}

		public AgentState ForPersonId(Guid personId)
		{
			return _data
				.FirstOrDefault(x => x.PersonId == personId);
		}

		public void Prepare(AgentStatePrepare model, DeadLockVictim deadLockVictim)
		{
			lock (_lock)
			{
				var copyFrom = _data.FirstOrDefault(x => x.PersonId == model.PersonId) ?? new AgentState();
				var adding = copyFrom.CopyBySerialization();
				adding.PersonId = model.PersonId;
				adding.BusinessUnitId = model.BusinessUnitId;
				adding.SiteId = model.SiteId;
				adding.TeamId = model.TeamId;
				_data = _data
					.Where(x => x.PersonId != model.PersonId)
					.Append(adding)
					.ToArray();
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
					.Select(x => JsonConvert.DeserializeObject<AgentState>(JsonConvert.SerializeObject(model)))
					.ToArray();

				_data = _data
					.Where(x => x.PersonId != model.PersonId)
					.Concat(updated)
					.ToArray();
			}
		}

		public virtual IEnumerable<PersonForCheck> FindForCheck()
		{
			lock (_lock)
				return _data
					.Select(x => new PersonForCheck
					{
						PersonId = x.PersonId,
						LastCheck = x.ReceivedTime,
						LastTimeWindowCheckSum = x.TimeWindowCheckSum
					})
					.ToArray();
		}

		public virtual IEnumerable<Guid> FindForClosingSnapshot(DateTime snapshotId, int snapshotDataSourceId, IEnumerable<Guid> loggedOutStateGroupIds)
		{
			lock (_lock)
				return _data
					.Where(s => s.SnapshotDataSourceId == snapshotDataSourceId && (s.SnapshotId < snapshotId || s.SnapshotId == null) && !loggedOutStateGroupIds.Contains(s.StateGroupId.Value))
					.Select(x => x.PersonId)
					.ToArray();
		}

		public virtual LockedData LockNLoad(IEnumerable<Guid> personIds, DeadLockVictim deadLockVictim)
		{
			lock (_lock)
				return new LockedData
				{
					AgentStates = _data
						.Where(x => personIds.Contains(x.PersonId))
						.ToArray(),
					MappingVersion = _keyValueStore.Get("RuleMappingsVersion"),
					ScheduleVersion = _keyValueStore.Get("CurrentScheduleReadModelVersion", CurrentScheduleReadModelVersion.Generate)
				};
		}

	}
}