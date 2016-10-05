using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;
using ExternalLogon = Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.ExternalLogon;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStatePersisterWithSchedules : AgentStatePersister
	{
		public AgentStatePersisterWithSchedules(ICurrentUnitOfWork unitOfWork, IJsonSerializer serializer) : base(unitOfWork, serializer)
		{
		}

		public override void Update(AgentState model)
		{
			var scheduleSql = "";
			if (model.Schedule != null)
				scheduleSql = ", Schedule = :Schedule";
			var sql = $@"
UPDATE [dbo].[AgentState]
SET
	BatchId = :BatchId,
	SourceId = :SourceId,
	PlatformTypeId = :PlatformTypeId,
	ReceivedTime = :ReceivedTime,
	StateCode = :StateCode,
	StateGroupId = :StateGroupId,
	StateStartTime = :StateStartTime,
	ActivityId = :ActivityId, 
	NextActivityId = :NextActivityId,
	NextActivityStartTime = :NextActivityStartTime, 
	RuleId = :RuleId,
	RuleStartTime = :RuleStartTime,
	AlarmStartTime = :AlarmStartTime,
	TimeWindowCheckSum = :TimeWindowCheckSum,
	Adherence = :Adherence
	{scheduleSql}
WHERE
	PersonId = :PersonId";
			var query = _unitOfWork.Current().Session()
				.CreateSQLQuery(sql)
				.SetParameter("PersonId", model.PersonId)
				.SetParameter("BatchId", model.BatchId)
				.SetParameter("SourceId", model.SourceId)
				.SetParameter("PlatformTypeId", model.PlatformTypeId)
				.SetParameter("ReceivedTime", model.ReceivedTime)
				.SetParameter("StateCode", model.StateCode)
				.SetParameter("StateGroupId", model.StateGroupId)
				.SetParameter("StateStartTime", model.StateStartTime)
				.SetParameter("ActivityId", model.ActivityId)
				.SetParameter("NextActivityId", model.NextActivityId)
				.SetParameter("NextActivityStartTime", model.NextActivityStartTime)
				.SetParameter("RuleId", model.RuleId)
				.SetParameter("RuleStartTime", model.RuleStartTime)
				.SetParameter("AlarmStartTime", model.AlarmStartTime)
				.SetParameter("TimeWindowCheckSum", model.TimeWindowCheckSum)
				.SetParameter("Adherence", (int?) model.Adherence);

			if (model.Schedule != null)
				query.SetParameter("Schedule", _serializer.SerializeObject(model.Schedule), NHibernateUtil.StringClob);

			query.ExecuteUpdate();
		}
	}

	public class AgentStatePersister : IAgentStatePersister
	{
		protected readonly ICurrentUnitOfWork _unitOfWork;
		protected readonly IJsonSerializer _serializer;

		public AgentStatePersister(ICurrentUnitOfWork unitOfWork, IJsonSerializer serializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
		}

		[LogInfo]
		public virtual void Prepare(AgentStatePrepare model, DeadLockVictim deadLockVictim)
		{
			setDeadLockPriority(deadLockVictim);

			if (model.ExternalLogons.IsNullOrEmpty())
			{
				_unitOfWork.Current().Session()
					.CreateSQLQuery("DELETE FROM [dbo].[AgentState] WHERE PersonId = :PersonId")
					.SetParameter("PersonId", model.PersonId)
					.ExecuteUpdate();
				return;
			}
			
			// select with upd lock to prevent deadlock
			var existing = _unitOfWork.Current().Session()
				.CreateSQLQuery(SelectAgentState + "WITH (UPDLOCK) WHERE PersonId = :PersonId")
				.SetParameter("PersonId", model.PersonId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalAgentState)))
				.List<AgentStateFound>();

			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"
DELETE FROM [dbo].[AgentState]
WHERE
	PersonId = :PersonId AND
	DataSourceIdUserCode NOT IN (:DataSourceIdUserCode)")
				.SetParameter("PersonId", model.PersonId)
				.SetParameterList("DataSourceIdUserCode", model.ExternalLogons.Select(x => $"{x.DataSourceId}__{x.UserCode}").ToArray())
				.ExecuteUpdate();

			_unitOfWork.Current().Session()
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

			var copyFrom = existing.FirstOrDefault();

			model.ExternalLogons
				.Where(e => !existing.Any(x => x.DataSourceId == e.DataSourceId && x.UserCode == e.UserCode))
				.ForEach(e =>
				{
					_unitOfWork.Current().Session()
						.CreateSQLQuery(@"
INSERT INTO [dbo].[AgentState]
(
	PersonId,
	BatchId,
	SourceId,
	PlatformTypeId,
	BusinessUnitId,
	SiteId,
	TeamId,
	ReceivedTime,
	StateCode,
	StateGroupId,
	StateStartTime,
	ActivityId,
	NextActivityId,
	NextActivityStartTime,
	RuleId,
	RuleStartTime,
	AlarmStartTime,
	TimeWindowCheckSum,
	Schedule,
	Adherence,
	DataSourceIdUserCode
)
VALUES
(
	:PersonId,
	:BatchId,
	:SourceId,
	:PlatformTypeId,
	:BusinessUnitId,
	:SiteId,
	:TeamId,
	:ReceivedTime,
	:StateCode,
	:StateGroupId,
	:StateStartTime,
	:ActivityId,
	:NextActivityId,
	:NextActivityStartTime,
	:RuleId,
	:RuleStartTime,
	:AlarmStartTime,
	:TimeWindowCheckSum,
	:Schedule,
	:Adherence,
	:DataSourceIdUserCode
)")
						.SetParameter("PersonId", model.PersonId)
						.SetParameter("BatchId", copyFrom?.BatchId)
						.SetParameter("SourceId", copyFrom?.SourceId)
						.SetParameter("PlatformTypeId", copyFrom?.PlatformTypeId)
						.SetParameter("BusinessUnitId", model.BusinessUnitId)
						.SetParameter("SiteId", model.SiteId)
						.SetParameter("TeamId", model.TeamId)
						.SetParameter("ReceivedTime", copyFrom?.ReceivedTime)
						.SetParameter("StateCode", copyFrom?.StateCode)
						.SetParameter("StateGroupId", copyFrom?.StateGroupId)
						.SetParameter("StateStartTime", copyFrom?.StateStartTime)
						.SetParameter("ActivityId", copyFrom?.ActivityId)
						.SetParameter("NextActivityId", copyFrom?.NextActivityId)
						.SetParameter("NextActivityStartTime", copyFrom?.NextActivityStartTime)
						.SetParameter("RuleId", copyFrom?.RuleId)
						.SetParameter("RuleStartTime", copyFrom?.RuleStartTime)
						.SetParameter("AlarmStartTime", copyFrom?.AlarmStartTime)
						.SetParameter("TimeWindowCheckSum", copyFrom?.TimeWindowCheckSum)
						.SetParameter("Schedule", copyFrom?.Schedule != null ? _serializer.SerializeObject(copyFrom.Schedule) : null, NHibernateUtil.StringClob)
						.SetParameter("Adherence", (int?) copyFrom?.Adherence)
						.SetParameter("DataSourceIdUserCode", e.DataSourceId + "__" + e.UserCode)
						.ExecuteUpdate();
				});
		}
		
		[LogInfo]
		public virtual void InvalidateSchedules(Guid personId, DeadLockVictim deadLockVictim)
		{
			setDeadLockPriority(deadLockVictim);

			_unitOfWork.Current().Session()
				.CreateSQLQuery("UPDATE [dbo].[AgentState] SET Schedule = NULL WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.ExecuteUpdate();
		}

		[LogInfo]
		public virtual void Update(AgentState model)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"
UPDATE [dbo].[AgentState]
SET
	BatchId = :BatchId,
	SourceId = :SourceId,
	PlatformTypeId = :PlatformTypeId,
	BusinessUnitId = :BusinessUnitId,
	SiteId = :SiteId,
	TeamId = :TeamId,
	ReceivedTime = :ReceivedTime,
	StateCode = :StateCode,
	StateGroupId = :StateGroupId,
	StateStartTime = :StateStartTime,
	ActivityId = :ActivityId, 
	NextActivityId = :NextActivityId,
	NextActivityStartTime = :NextActivityStartTime, 
	RuleId = :RuleId,
	RuleStartTime = :RuleStartTime,
	AlarmStartTime = :AlarmStartTime,
	TimeWindowCheckSum = :TimeWindowCheckSum,
	Adherence = :Adherence
WHERE
	PersonId = :PersonId")
				.SetParameter("PersonId", model.PersonId)
				.SetParameter("BatchId", model.BatchId)
				.SetParameter("SourceId", model.SourceId)
				.SetParameter("PlatformTypeId", model.PlatformTypeId)
				.SetParameter("BusinessUnitId", model.BusinessUnitId)
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("TeamId", model.TeamId)
				.SetParameter("ReceivedTime", model.ReceivedTime)
				.SetParameter("StateCode", model.StateCode)
				.SetParameter("StateGroupId", model.StateGroupId)
				.SetParameter("StateStartTime", model.StateStartTime)
				.SetParameter("ActivityId", model.ActivityId)
				.SetParameter("NextActivityId", model.NextActivityId)
				.SetParameter("NextActivityStartTime", model.NextActivityStartTime)
				.SetParameter("RuleId", model.RuleId)
				.SetParameter("RuleStartTime", model.RuleStartTime)
				.SetParameter("AlarmStartTime", model.AlarmStartTime)
				.SetParameter("TimeWindowCheckSum", model.TimeWindowCheckSum)
				.SetParameter("Adherence", (int?) model.Adherence)
				.ExecuteUpdate();
		}

		public void Delete(Guid personId, DeadLockVictim deadLockVictim)
		{
			setDeadLockPriority(deadLockVictim);

			_unitOfWork.Current().Session()
				.CreateSQLQuery("DELETE [dbo].[AgentState] WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.ExecuteUpdate()
				;
		}

		[LogInfo]
		public virtual IEnumerable<ExternalLogon> FindAll()
		{
			var sql = "SELECT PersonId, DataSourceIdUserCode FROM [dbo].[AgentState] WITH (NOLOCK)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalExternalLogon)))
				.SetReadOnly(true)
				.List<internalExternalLogon>()
				.GroupBy(x => x.PersonId, (guid, states) => states.First())
				.ToArray()
				;
		}

		[LogInfo]
		public virtual IEnumerable<ExternalLogon> FindForClosingSnapshot(DateTime snapshotId, string sourceId, string loggedOutState)
		{
			return _unitOfWork.Current().Session().CreateSQLQuery(@"
SELECT
	PersonId,
	DataSourceIdUserCode
FROM [dbo].[AgentState] WITH (NOLOCK)
WHERE
	SourceId = :SourceId AND 
	(BatchId < :SnapshotId OR BatchId IS NULL) AND
	(StateCode <> :State OR StateCode IS NULL)")
				.SetParameter("SourceId", sourceId)
				.SetParameter("SnapshotId", snapshotId)
				.SetParameter("State", loggedOutState)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalExternalLogon)))
				.SetReadOnly(true)
				.List<internalExternalLogon>()
				.GroupBy(x => x.PersonId, (guid, states) => states.First())
				.ToArray();
		}

		[LogInfo]
		public virtual IEnumerable<AgentStateFound> Find(ExternalLogon externalLogon, DeadLockVictim deadLockVictim)
		{
			setDeadLockPriority(deadLockVictim);

			var sql = SelectAgentState + "WITH (UPDLOCK, ROWLOCK) WHERE DataSourceIdUserCode = :DataSourceIdUserCode";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameter("DataSourceIdUserCode", $"{externalLogon.DataSourceId}__{externalLogon.UserCode}")
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalAgentState)))
				.SetReadOnly(true)
				.List<AgentStateFound>()
				;
		}

		[LogInfo]
		public virtual IEnumerable<AgentStateFound> Find(IEnumerable<ExternalLogon> externalLogons, DeadLockVictim deadLockVictim)
		{
			setDeadLockPriority(deadLockVictim);

			var dataSourceIdUserCodes = externalLogons.Select(x => $"{x.DataSourceId}__{x.UserCode}").ToArray();
			var sql = SelectAgentState + "WITH (UPDLOCK, ROWLOCK) WHERE DataSourceIdUserCode IN (:DataSourceIdUserCodes)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameterList("DataSourceIdUserCodes", dataSourceIdUserCodes)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalAgentState)))
				.SetReadOnly(true)
				.List<AgentStateFound>()
				;
		}

		[LogInfo]
		public virtual IEnumerable<AgentState> GetStates()
		{
			var sql = SelectAgentState + "WITH (TABLOCK, UPDLOCK)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalAgentState)))
				.SetReadOnly(true)
				.List<AgentState>()
				.GroupBy(x => x.PersonId, (guid, states) => states.First())
				.ToArray()
				;
		}

		[LogInfo]
		public virtual AgentState Get(Guid personId)
		{
			var sql = SelectAgentState + "WITH (UPDLOCK, ROWLOCK) WHERE PersonId = :PersonId";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameter("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalAgentState)))
				.SetReadOnly(true)
				.List<AgentState>()
				.FirstOrDefault();
		}

		[LogInfo]
		public virtual IEnumerable<AgentState> GetStatesNotInSnapshot(DateTime snapshotId, string sourceId)
		{
			var sql = SelectAgentState +
					  @"WITH (UPDLOCK) WHERE 
						SourceId = :SourceId
						AND (
							BatchId < :SnapshotId
							OR 
							BatchId IS NULL
							)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameter("SnapshotId", snapshotId)
				.SetParameter("SourceId", sourceId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalAgentState)))
				.SetReadOnly(true)
				.List<AgentState>()
				.GroupBy(x => x.PersonId, (guid, states) => states.First())
				.ToArray()
				;
		}

		[LogInfo]
		public virtual IEnumerable<AgentState> Get(IEnumerable<Guid> personIds)
		{
			var sql = SelectAgentState + "WITH (UPDLOCK) WHERE PersonId IN (:PersonIds)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameterList("PersonIds", personIds)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalAgentState)))
				.SetReadOnly(true)
				.List<AgentState>()
				.GroupBy(x => x.PersonId, (guid, states) => states.First())
				.ToArray()
				;
		}

		private void setDeadLockPriority(DeadLockVictim deadLockVictim)
		{
			var sql = "SET DEADLOCK_PRIORITY " + (deadLockVictim == DeadLockVictim.Yes ? "LOW" : "HIGH");
			_unitOfWork.Current().Session().CreateSQLQuery(sql).ExecuteUpdate();
		}

		private static string SelectAgentState = @"SELECT * FROM [dbo].[AgentState] ";

		private class internalAgentState : AgentStateFound
		{
			public new string Schedule { set
			{
				base.Schedule = value != null ? JsonConvert.DeserializeObject<IEnumerable<ScheduledActivity>>(value) : null;
			} }

			public new int? Adherence
			{
				set { base.Adherence = (EventAdherence?) value; }
			}

			public string DataSourceIdUserCode
			{
				set
				{
					if (string.IsNullOrEmpty(value))
						return;
					var x = value.Split('_');
					DataSourceId = int.Parse(x[0]);
					UserCode = x[2];
				}
			}
		}

		private class internalExternalLogon : ExternalLogon
		{
			public Guid PersonId { get; set; }

			public string DataSourceIdUserCode
			{
				set
				{
					if (string.IsNullOrEmpty(value))
						return;
					var x = value.Split('_');
					DataSourceId = int.Parse(x[0]);
					UserCode = x[2];
				}
			}
		}
	}
}