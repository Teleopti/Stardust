using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStatePersister : IAgentStatePersister
	{
		private readonly ICurrentAnalyticsUnitOfWork _unitOfWork;

		public AgentStatePersister(ICurrentAnalyticsUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Persist(AgentState model)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(
					@"EXEC [RTA].[rta_addorupdate_actualagentstate]
					@PersonId = :PersonId,
					@BatchId = :BatchId,
					@BusinessUnitId = :BusinessUnitId,
					@SiteId = :SiteId,
					@TeamId = :TeamId,
					@OriginalDataSourceId = :OriginalDataSourceId,
					@PlatformTypeId = :PlatformTypeId,
					@ReceivedTime = :ReceivedTime,
					@Scheduled = :Scheduled,
					@ScheduledId = :ScheduledId,
					@ScheduledNext = :ScheduledNext,
					@ScheduledNextId = :ScheduledNextId,
					@NextStart = :NextStart,
					@StateCode = :StateCode,
					@State = :State,
					@StateId = :StateId,
					@StateStartTime = :StateStartTime,
					@AlarmId = :AlarmId,
					@AlarmName = :AlarmName,
					@RuleStartTime = :RuleStartTime,
					@Color = :Color,
					@StaffingEffect = :StaffingEffect,
					@Adherence = :Adherence,
					@IsRuleAlarm = :IsRuleAlarm,
					@AlarmStartTime = :AlarmStartTime,
					@AlarmColor = :AlarmColor")
				.SetParameter("PersonId", model.PersonId)
				.SetParameter("BatchId", model.BatchId)
				.SetParameter("BusinessUnitId", model.BusinessUnitId)
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("TeamId", model.TeamId)
				.SetParameter("OriginalDataSourceId", model.SourceId)
				.SetParameter("PlatformTypeId", model.PlatformTypeId)
				.SetParameter("ReceivedTime", model.ReceivedTime)
				.SetParameter("Scheduled", "")
				.SetParameter("ScheduledId", Guid.NewGuid())
				.SetParameter("ScheduledNext", "")
				.SetParameter("ScheduledNextId", Guid.NewGuid())
				.SetParameter("NextStart", DateTime.UtcNow)
				.SetParameter("StateCode", model.StateCode)
				.SetParameter("State", "")
				.SetParameter("StateId", model.StateGroupId)
				.SetParameter("StateStartTime", model.StateStartTime)
				.SetParameter("AlarmId", model.RuleId)
				.SetParameter("AlarmName", "")
				.SetParameter("RuleStartTime", model.RuleStartTime)
				.SetParameter("Color", 0)
				.SetParameter("StaffingEffect", model.StaffingEffect)
				.SetParameter("Adherence", model.Adherence)
				.SetParameter("IsRuleAlarm", false)
				.SetParameter("AlarmStartTime", model.AlarmStartTime)
				.SetParameter("AlarmColor", 0)
				.ExecuteUpdate();
		}

		public void Delete(Guid personId)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(
					@"DELETE RTA.ActualAgentState WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.ExecuteUpdate()
				;
		}

		public virtual IEnumerable<AgentState> GetAll()
		{
			var sql = AgentStateReadModelReader.SelectActualAgentState + "WITH (TABLOCK UPDLOCK)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AgentStateReadModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>()
				.Select(x => new AgentState(x));
		}

		[InfoLog]
		public virtual AgentState Get(Guid personId)
		{
			var sql = AgentStateReadModelReader.SelectActualAgentState + "WITH (UPDLOCK) WHERE PersonId = :PersonId";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameter("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AgentStateReadModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>()
				.Select(x => new AgentState(x))
				.FirstOrDefault();
		}

		[InfoLog]
		public virtual IEnumerable<AgentState> GetNotInSnapshot(DateTime batchId, string dataSourceId)
		{
			var sql = AgentStateReadModelReader.SelectActualAgentState +
					  @"WITH (UPDLOCK) WHERE 
						OriginalDataSourceId = :OriginalDataSourceId
						AND (
							BatchId < :BatchId
							OR 
							BatchId IS NULL
							)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameter("BatchId", batchId)
				.SetParameter("OriginalDataSourceId", dataSourceId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (AgentStateReadModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>()
				.Select(x => new AgentState(x));
		}
	}
}