using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Multi;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.States.Infrastructure
{
	public class AgentStatePersister : IAgentStatePersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly DeadLockVictimPriority _deadLockVictimPriority;

		public AgentStatePersister(
			ICurrentUnitOfWork unitOfWork,
			DeadLockVictimPriority deadLockVictimPriority)
		{
			_unitOfWork = unitOfWork;
			_deadLockVictimPriority = deadLockVictimPriority;
		}

		[LogInfo]
		public virtual void Prepare(AgentStatePrepare model, DeadLockVictim deadLockVictim)
		{
			_deadLockVictimPriority.Specify(deadLockVictim);

			var updated = _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
UPDATE [dbo].[AgentState]
SET
	BusinessUnitId = :BusinessUnitId,
	SiteId = :SiteId,
	TeamId = :TeamId
WHERE
	PersonId = :PersonId")
				.SetParameter("BusinessUnitId", model.BusinessUnitId)
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("TeamId", model.TeamId)
				.SetParameter("PersonId", model.PersonId)
				.ExecuteUpdate();

			if (updated == 0)
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
INSERT INTO [dbo].[AgentState]
(
	PersonId,
	BusinessUnitId,
	SiteId,
	TeamId
)
VALUES
(
	:PersonId,
	:BusinessUnitId,
	:SiteId,
	:TeamId
)")
					.SetParameter("PersonId", model.PersonId)
					.SetParameter("BusinessUnitId", model.BusinessUnitId)
					.SetParameter("SiteId", model.SiteId)
					.SetParameter("TeamId", model.TeamId)
					.ExecuteUpdate();
		}

		[LogInfo]
		public virtual void Update(AgentState model)
		{
			var sql = @"
UPDATE [dbo].[AgentState]
SET
	SnapshotId = :SnapshotId,
	SnapshotDataSourceId = :SnapshotDataSourceId,
	ReceivedTime = :ReceivedTime,
	StateGroupId = :StateGroupId,
	StateStartTime = :StateStartTime,
	ActivityId = :ActivityId, 
	RuleId = :RuleId,
	RuleStartTime = :RuleStartTime,
	AlarmStartTime = :AlarmStartTime,
	TimeWindowCheckSum = :TimeWindowCheckSum,
	Adherence = :Adherence
WHERE
	PersonId = :PersonId";
			var query = _unitOfWork.Current().Session()
					.CreateSQLQuery(sql)
					.SetParameter("PersonId", model.PersonId)
					.SetParameter("SnapshotId", model.SnapshotId.GetValueOrDefault().Ticks)
					.SetParameter("SnapshotDataSourceId", model.SnapshotDataSourceId)
					.SetParameter("ReceivedTime", model.ReceivedTime)
					.SetParameter("StateGroupId", model.StateGroupId)
					.SetParameter("StateStartTime", model.StateStartTime)
					.SetParameter("ActivityId", model.ActivityId)
					.SetParameter("RuleId", model.RuleId)
					.SetParameter("RuleStartTime", model.RuleStartTime)
					.SetParameter("AlarmStartTime", model.AlarmStartTime)
					.SetParameter("TimeWindowCheckSum", model.TimeWindowCheckSum)
					.SetParameter("Adherence", (int?) model.Adherence)
				;

			query.ExecuteUpdate();
		}

		public void Delete(Guid personId, DeadLockVictim deadLockVictim)
		{
			_deadLockVictimPriority.Specify(deadLockVictim);

			_unitOfWork.Current().Session()
				.CreateSQLQuery("DELETE [dbo].[AgentState] WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.ExecuteUpdate()
				;
		}

		[LogInfo]
		public virtual IEnumerable<PersonForCheck> FindForCheck()
		{
			var sql = @"
SELECT
	PersonId,
	ReceivedTime AS LastCheck,
	TimeWindowCheckSum AS LastTimeWindowCheckSum
FROM 
	[dbo].[AgentState] WITH (NOLOCK)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetResultTransformer(Transformers.AliasToBean<PersonForCheck>())
				.SetReadOnly(true)
				.List<PersonForCheck>();
		}

		[LogInfo]
		public virtual IEnumerable<Guid> FindForClosingSnapshot(DateTime snapshotId, int snapshotDataSourceId, IEnumerable<Guid> loggedOutStateGroupIds)
		{
			var stateGroups = loggedOutStateGroupIds.IsEmpty() ? "IS NOT NULL" : "NOT IN (:StateGroupIds)";

			return _unitOfWork.Current().Session().CreateSQLQuery($@"
SELECT PersonId
FROM [dbo].[AgentState] WITH (NOLOCK)
WHERE SnapshotDataSourceId = :SnapshotDataSourceId
AND (SnapshotId < :SnapshotId OR SnapshotId IS NULL)
AND StateGroupId {stateGroups}")
				.SetParameter("SnapshotDataSourceId", snapshotDataSourceId)
				.SetParameter("SnapshotId", snapshotId.Ticks)
				.SetParameterListIf(loggedOutStateGroupIds.Any(), "StateGroupIds", loggedOutStateGroupIds)
				.SetReadOnly(true)
				.List<Guid>();
		}

		[LogInfo]
		public virtual LockedData LockNLoad(IEnumerable<Guid> personIds, DeadLockVictim deadLockVictim)
		{
			_deadLockVictimPriority.Specify(deadLockVictim);

			var sql = SelectAgentState + "WITH (UPDLOCK, ROWLOCK) WHERE PersonId IN (:PersonIds)";

			var agentStateQuery = _unitOfWork.Current().Session()
				.CreateSQLQuery(sql)
				.SetParameterList("PersonIds", personIds)
				.SetResultTransformer(Transformers.AliasToBean<internalAgentState>())
				.SetReadOnly(true);
			// shared lock for versions so we wait for running updaters
			var keyValueQuery = _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT * FROM [ReadModel].[KeyValueStore] WHERE [Key] IN ('RuleMappingsVersion', 'CurrentScheduleReadModelVersion')")
				.SetResultTransformer(Transformers.AliasToBean<internalKeyValue>())
				.SetReadOnly(true);

			var results = _unitOfWork.Current().Session().CreateQueryBatch()
				.Add<internalAgentState>(agentStateQuery)
				.Add<internalKeyValue>(keyValueQuery);

			var agentStates = results.GetResult<internalAgentState>(0);
			var keyValues = results.GetResult<internalKeyValue>(1);

			return new LockedData
			{
				AgentStates =
					agentStates
						.GroupBy(x => x.PersonId, (guid, states) => states.First())
						.ToArray(),
				MappingVersion = keyValues.SingleOrDefault(x => x.Key == "RuleMappingsVersion")?.Value,
				ScheduleVersion = parseVersion(keyValues.SingleOrDefault(x => x.Key == "CurrentScheduleReadModelVersion")?.Value)
			};
		}

		private CurrentScheduleReadModelVersion parseVersion(string version)
		{
			return CurrentScheduleReadModelVersion.Parse(version);
		}

		private static string SelectAgentState = "SELECT * FROM [dbo].[AgentState] ";

		private class internalKeyValue
		{
			public string Key { get; set; }
			public string Value { get; set; }
		}

		private class internalAgentState : AgentState
		{
			public new long? SnapshotId
			{
				set { base.SnapshotId = value == null ? (DateTime?) null : new DateTime(value.Value); }
			}

			public new int? Adherence
			{
				set { base.Adherence = (EventAdherence?) value; }
			}
		}
	}
}