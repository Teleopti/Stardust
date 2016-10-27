using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;
using ExternalLogon = Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.ExternalLogon;

namespace Teleopti.Ccc.Infrastructure.Rta
{
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
				.SetParameterList("DataSourceIdUserCode",
					model.ExternalLogons.Select(x => $"{x.DataSourceId}__{x.UserCode}").ToArray())
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
	RuleId,
	RuleStartTime,
	AlarmStartTime,
	TimeWindowCheckSum,
	Adherence,
	Schedule,
	NextCheck,
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
	:RuleId,
	:RuleStartTime,
	:AlarmStartTime,
	:TimeWindowCheckSum,
	:Adherence,
	:Schedule,
	:NextCheck,
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
						.SetParameter("RuleId", copyFrom?.RuleId)
						.SetParameter("RuleStartTime", copyFrom?.RuleStartTime)
						.SetParameter("AlarmStartTime", copyFrom?.AlarmStartTime)
						.SetParameter("TimeWindowCheckSum", copyFrom?.TimeWindowCheckSum)
						.SetParameter("Adherence", (int?)copyFrom?.Adherence)
						.SetParameter("Schedule", copyFrom?.Schedule != null ? _serializer.SerializeObject(copyFrom.Schedule) : null, NHibernateUtil.StringClob)
						.SetParameter("NextCheck", copyFrom?.NextCheck)
						.SetParameter("DataSourceIdUserCode", e.DataSourceId + "__" + e.UserCode)
						.ExecuteUpdate();
				});
		}

		[LogInfo]
		public virtual void InvalidateSchedules(Guid personId, DeadLockVictim deadLockVictim)
		{
			setDeadLockPriority(deadLockVictim);

			_unitOfWork.Current().Session()
				.CreateSQLQuery("UPDATE [dbo].[AgentState] SET Schedule = NULL, NextCheck = NULL WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.ExecuteUpdate();
		}

		[LogInfo]
		public virtual void Update(AgentState model, bool updateSchedule)
		{
			var scheduleSql = "";

			if (updateSchedule)
				scheduleSql = "Schedule = :Schedule,";

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
	RuleId = :RuleId,
	RuleStartTime = :RuleStartTime,
	AlarmStartTime = :AlarmStartTime,
	TimeWindowCheckSum = :TimeWindowCheckSum,
	Adherence = :Adherence,
	{scheduleSql}
	NextCheck = :NextCheck
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
				.SetParameter("RuleId", model.RuleId)
				.SetParameter("RuleStartTime", model.RuleStartTime)
				.SetParameter("AlarmStartTime", model.AlarmStartTime)
				.SetParameter("TimeWindowCheckSum", model.TimeWindowCheckSum)
				.SetParameter("Adherence", (int?) model.Adherence)
				.SetParameter("NextCheck", model.NextCheck)
				;

			if (updateSchedule)
				query.SetParameter("Schedule", _serializer.SerializeObject(model.Schedule), NHibernateUtil.StringClob);

			throwDeadlockVictimExceptionFor(() => query.ExecuteUpdate());
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
		public virtual IEnumerable<ExternalLogonForCheck> FindForCheck()
		{
			return _unitOfWork.Current().Session().CreateSQLQuery(@"
SELECT
	PersonId,
	DataSourceIdUserCode,
	NextCheck
FROM [dbo].[AgentState] WITH (NOLOCK)
")
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalExternalLogonForCheck)))
				.SetReadOnly(true)
				.List<internalExternalLogonForCheck>()
				.GroupBy(x => x.PersonId, (guid, states) => states.First())
				.ToArray();
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
		public virtual IEnumerable<AgentStateFound> Find(IEnumerable<ExternalLogon> externalLogons, DeadLockVictim deadLockVictim)
		{
			setDeadLockPriority(deadLockVictim);

			var dataSourceIdUserCodes = externalLogons.Select(x => $"{x.DataSourceId}__{x.UserCode}").ToArray();
			var sql = SelectAgentState + "WITH (UPDLOCK, ROWLOCK) WHERE DataSourceIdUserCode IN (:DataSourceIdUserCodes)";

			IEnumerable<AgentStateFound> result = null;
			throwDeadlockVictimExceptionFor(() =>
			{
				result = _unitOfWork.Current().Session().CreateSQLQuery(sql)
					.SetParameterList("DataSourceIdUserCodes", dataSourceIdUserCodes)
					.SetResultTransformer(Transformers.AliasToBean(typeof(internalAgentState)))
					.SetReadOnly(true)
					.List<AgentStateFound>()
					;
			});
			return result;
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

		private static void throwDeadlockVictimExceptionFor(Action action)
		{
			try
			{
				action.Invoke();
			}
			catch (DataSourceException e)
			{
				var sqlDeadlockException = e.AllExceptions().FirstOrDefault(x => x.IsSqlDeadlock());
				if (sqlDeadlockException != null)
					throw new DeadLockVictimException("Transaction deadlocked updating AgentState", sqlDeadlockException);
				throw;
			}
		}

		private static string SelectAgentState = @"SELECT * FROM [dbo].[AgentState] ";

		private static void parse(string value, Action<Tuple<int, string>> split)
		{
			if (string.IsNullOrEmpty(value))
				return;
			var arr = value.Split('_');
			split.Invoke(new Tuple<int, string>(
				int.Parse(arr[0]),
				arr[2]
			));
		}

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
					parse(value, x =>
					{
						DataSourceId = x.Item1;
						UserCode = x.Item2;
					});
				}
			}
		}

		private class internalExternalLogonForCheck : ExternalLogonForCheck
		{
			public Guid PersonId { get; set; }

			public string DataSourceIdUserCode
			{
				set
				{
					parse(value, x =>
					{
						DataSourceId = x.Item1;
						UserCode = x.Item2;
					});
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
					parse(value, x =>
					{
						DataSourceId = x.Item1;
						UserCode = x.Item2;
					});
				}
			}
		}
	}
}