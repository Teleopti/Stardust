using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStatePersister : IAgentStatePersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly DeadLockVictimThrower _deadLockVictimThrower;

		public AgentStatePersister(
			ICurrentUnitOfWork unitOfWork,
			DeadLockVictimThrower deadLockVictimThrower)
		{
			_unitOfWork = unitOfWork;
			_deadLockVictimThrower = deadLockVictimThrower;
		}

		[LogInfo]
		public virtual void Prepare(AgentStatePrepare model, DeadLockVictim deadLockVictim)
		{
			_deadLockVictimThrower.SetDeadLockPriority(deadLockVictim);

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
	PlatformTypeId = :PlatformTypeId,
	ReceivedTime = :ReceivedTime,
	StateCode = :StateCode,
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
					.SetParameter("SnapshotId", model.SnapshotId)
					.SetParameter("SnapshotDataSourceId", model.SnapshotDataSourceId)
					.SetParameter("PlatformTypeId", model.PlatformTypeId)
					.SetParameter("ReceivedTime", model.ReceivedTime)
					.SetParameter("StateCode", model.StateCode)
					.SetParameter("StateGroupId", model.StateGroupId)
					.SetParameter("StateStartTime", model.StateStartTime)
					.SetParameter("ActivityId", model.ActivityId)
					.SetParameter("RuleId", model.RuleId)
					.SetParameter("RuleStartTime", model.RuleStartTime)
					.SetParameter("AlarmStartTime", model.AlarmStartTime)
					.SetParameter("TimeWindowCheckSum", model.TimeWindowCheckSum)
					.SetParameter("Adherence", (int?) model.Adherence)
				;

			_deadLockVictimThrower.ThrowOnDeadlock(() => query.ExecuteUpdate());
		}

		public void Delete(Guid personId, DeadLockVictim deadLockVictim)
		{
			_deadLockVictimThrower.SetDeadLockPriority(deadLockVictim);

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
		public virtual IEnumerable<Guid> FindForClosingSnapshot(DateTime snapshotId, int snapshotDataSourceId, string loggedOutState)
		{
			return _unitOfWork.Current().Session().CreateSQLQuery(@"
SELECT
	PersonId
FROM [dbo].[AgentState] WITH (NOLOCK)
WHERE SnapshotDataSourceId = :SnapshotDataSourceId
AND (SnapshotId < :SnapshotId OR SnapshotId IS NULL)
AND (StateCode <> :State OR StateCode IS NULL)")
				.SetParameter("SnapshotDataSourceId", snapshotDataSourceId)
				.SetParameter("SnapshotId", snapshotId)
				.SetParameter("State", loggedOutState)
				.SetReadOnly(true)
				.List<Guid>();
		}

		[LogInfo]
		public virtual LockedData LockNLoad(IEnumerable<Guid> personIds, DeadLockVictim deadLockVictim)
		{
			_deadLockVictimThrower.SetDeadLockPriority(deadLockVictim);

			var sql = SelectAgentState + "WITH (UPDLOCK, ROWLOCK) WHERE PersonId IN (:PersonIds)";

			return _deadLockVictimThrower.ThrowOnDeadlock(() =>
			{
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

				var results = _unitOfWork.Current().Session().CreateMultiQuery()
					.Add(agentStateQuery)
					.Add(keyValueQuery)
					.List();

				var agentStates = (results[0] as IEnumerable<object>).Cast<AgentState>();
				var keyValues = (results[1] as IEnumerable<object>).Cast<internalKeyValue>();

				return new LockedData
				{
					AgentStates =
						agentStates
							.GroupBy(x => x.PersonId, (guid, states) => states.First())
							.ToArray(),
					MappingVersion = keyValues.SingleOrDefault(x => x.Key == "RuleMappingsVersion")?.Value,
					ScheduleVersion = keyValues.SingleOrDefault(x => x.Key == "CurrentScheduleReadModelVersion")?.Value
				};
			});
		}

		private static string SelectAgentState = "SELECT * FROM [dbo].[AgentState] ";
		
		private class internalKeyValue
		{
			public string Key { get; set; }
			public string Value { get; set; }
		}

		private class internalAgentState : AgentState
		{
			public new int? Adherence { set { base.Adherence = (EventAdherence?) value; } }
		}
	}
}