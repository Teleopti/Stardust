using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;

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
	TimeWindowCheckSum = :TimeWindowCheckSum
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
				.SetParameter("TimeWindowCheckSum", model.TimeWindowCheckSum);

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

		[InfoLog]
		public virtual void Prepare(AgentStatePrepare model)
		{
			if (model.ExternalLogons.IsNullOrEmpty())
			{
				_unitOfWork.Current().Session()
					.CreateSQLQuery("DELETE FROM [dbo].[AgentState] WHERE PersonId = :PersonId")
					.SetParameter("PersonId", model.PersonId)
					.ExecuteUpdate();
				return;
			}

			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"
DELETE FROM [dbo].[AgentState]
WHERE
	PersonId = :PersonId AND
	CAST(DataSourceId AS varchar(max)) + ';' + UserCode NOT IN (:DataSourceIdUserCode)")
				.SetParameter("PersonId", model.PersonId)
				.SetParameterList("DataSourceIdUserCode", model.ExternalLogons.Select(x => $"{x.DataSourceId};{x.UserCode}"))
				.ExecuteUpdate();

			// select with upd lock to prevent deadlock
			var existing = _unitOfWork.Current().Session()
				.CreateSQLQuery(SelectAgentState + "WITH (UPDLOCK) WHERE PersonId = :PersonId")
				.SetParameter("PersonId", model.PersonId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalAgentState)))
				.List<AgentStateFound>();

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

			model.ExternalLogons
				.Where(e => !existing.Any(x => x.DataSourceId == e.DataSourceId && x.UserCode == e.UserCode))
				.ForEach(e =>
				{
					_unitOfWork.Current().Session()
						.CreateSQLQuery(@"
INSERT INTO [dbo].[AgentState] (BusinessUnitId, SiteId, TeamId, PersonId, DataSourceId, UserCode)
VALUES (:BusinessUnitId, :SiteId, :TeamId, :PersonId, :DataSourceId, :UserCode)")
						.SetParameter("BusinessUnitId", model.BusinessUnitId)
						.SetParameter("SiteId", model.SiteId)
						.SetParameter("TeamId", model.TeamId)
						.SetParameter("PersonId", model.PersonId)
						.SetParameter("DataSourceId", e.DataSourceId)
						.SetParameter("UserCode", e.UserCode)
						.ExecuteUpdate();
				});
		}
		
		[InfoLog]
		public virtual void InvalidateSchedules(Guid personId)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery("UPDATE [dbo].[AgentState] SET Schedule = NULL WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.ExecuteUpdate();
		}

		[InfoLog]
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
	TimeWindowCheckSum = :TimeWindowCheckSum
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
				.ExecuteUpdate();
		}

		public void Delete(Guid personId)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(
					@"DELETE [dbo].[AgentState] WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.ExecuteUpdate()
				;
		}
		
		[InfoLog]
		public virtual AgentState Get(Guid personId)
		{
			var sql = SelectAgentState + "WITH (UPDLOCK) WHERE PersonId = :PersonId";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameter("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalAgentState)))
				.SetReadOnly(true)
				.List<AgentState>()
				.FirstOrDefault();
		}

		[InfoLog]
		public virtual IEnumerable<AgentStateFound> Find(int dataSourceId, string userCode)
		{
			var sql = SelectAgentState + "WITH (UPDLOCK) WHERE DataSourceId = :DataSourceId AND UserCode = :UserCode";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameter("DataSourceId", dataSourceId)
				.SetParameter("UserCode", userCode)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalAgentState)))
				.SetReadOnly(true)
				.List<AgentStateFound>()
				;
		}

		[InfoLog]
		public virtual IEnumerable<AgentStateFound> Find(int dataSourceId, IEnumerable<string> userCodes)
		{
			var sql = SelectAgentState + "WITH (UPDLOCK) WHERE DataSourceId = :DataSourceId AND UserCode IN (:UserCodes)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameter("DataSourceId", dataSourceId)
				.SetParameterList("UserCodes", userCodes)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalAgentState)))
				.SetReadOnly(true)
				.List<AgentStateFound>()
				;
		}

		[InfoLog]
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

		[InfoLog]
		public virtual IEnumerable<Guid> GetPersonIdsForClosingSnapshot(DateTime snapshotId, string sourceId, string loggedOutState)
		{
			return _unitOfWork.Current().Session().CreateSQLQuery(@"
SELECT DISTINCT
	PersonId
FROM [dbo].[AgentState] WITH (NOLOCK)
WHERE
	SourceId = :SourceId AND 
	(BatchId < :SnapshotId OR BatchId IS NULL) AND
	(StateCode <> :State OR StateCode IS NULL)")
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.SetParameter("SourceId", sourceId)
				.SetParameter("SnapshotId", snapshotId)
				.SetParameter("State", loggedOutState)
				.SetReadOnly(true)
				.List<Guid>()
				.ToArray()
				;
		}

		[InfoLog]
		public virtual IEnumerable<Guid> GetAllPersonIds()
		{
			var sql = @"SELECT DISTINCT PersonId FROM [dbo].[AgentState] WITH (NOLOCK)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.SetReadOnly(true)
				.List<Guid>()
				.ToArray()
				;
		}

		[InfoLog]
		public virtual IEnumerable<AgentState> GetStates()
		{
			var sql = SelectAgentState + "WITH (TABLOCK UPDLOCK)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalAgentState)))
				.SetReadOnly(true)
				.List<AgentState>()
				.GroupBy(x => x.PersonId, (guid, states) => states.First())
				.ToArray()
				;
		}

		[InfoLog]
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
		
		private static string SelectAgentState = @"SELECT * FROM [dbo].[AgentState] ";

		private class internalAgentState : AgentStateFound
		{
			public new string Schedule { set
			{
				base.Schedule = value != null ? JsonConvert.DeserializeObject<IEnumerable<ScheduledActivity>>(value) : null;
			} }
		}
	}
}