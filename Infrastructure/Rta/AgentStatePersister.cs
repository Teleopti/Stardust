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
		private readonly ICurrentUnitOfWork _unitOfWork;

		public AgentStatePersister(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		[InfoLog]
		public virtual void Persist(AgentState model)
		{
			var updated = _unitOfWork.Current().Session()
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
						StaffingEffect = :StaffingEffect,
						Adherence = :Adherence,
						AlarmStartTime = :AlarmStartTime
					WHERE 
						PersonId = :PersonId
				")
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
				.SetParameter("StaffingEffect", model.StaffingEffect)
				.SetParameter("Adherence", model.Adherence)
				.SetParameter("AlarmStartTime", model.AlarmStartTime)
				.ExecuteUpdate();
			if (updated == 0)
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
							TeamId,
							SiteId,
							ReceivedTime,
							StateCode,
							StateGroupId,
							StateStartTime,
							ActivityId,
							NextActivityId,
							NextActivityStartTime,
							RuleId,
							RuleStartTime,
							StaffingEffect,
							Adherence,
							AlarmStartTime)
						VALUES
						(
							:PersonId, 
							:BatchId,
							:SourceId,
							:PlatformTypeId,
							:BusinessUnitId,
							:TeamId,
							:SiteId,
							:ReceivedTime,
							:StateCode,
							:StateGroupId,
							:StateStartTime,
							:ActivityId,
							:NextActivityId,
							:NextActivityStartTime,
							:RuleId,
							:RuleStartTime,
							:StaffingEffect,
							:Adherence,
							:AlarmStartTime)
					")
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
					.SetParameter("StaffingEffect", model.StaffingEffect)
					.SetParameter("Adherence", model.Adherence)
					.SetParameter("AlarmStartTime", model.AlarmStartTime)
					.ExecuteUpdate();
			}
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

		public virtual IEnumerable<AgentState> GetAll()
		{
			var sql = SelectAgentState + "WITH (TABLOCK UPDLOCK)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalState)))
				.SetReadOnly(true)
				.List<AgentState>();
		}

		[InfoLog]
		public virtual AgentState Get(Guid personId)
		{
			var sql = SelectAgentState + "WITH (UPDLOCK) WHERE PersonId = :PersonId";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameter("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalState)))
				.SetReadOnly(true)
				.List<AgentState>()
				.FirstOrDefault();
		}

		[InfoLog]
		public virtual IEnumerable<AgentState> GetNotInSnapshot(DateTime batchId, string sourceId)
		{
			var sql = SelectAgentState +
					  @"WITH (UPDLOCK) WHERE 
						SourceId = :SourceId
						AND (
							BatchId < :BatchId
							OR 
							BatchId IS NULL
							)";
			return _unitOfWork.Current().Session().CreateSQLQuery(sql)
				.SetParameter("BatchId", batchId)
				.SetParameter("SourceId", sourceId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalState)))
				.SetReadOnly(true)
				.List<AgentState>();
		}

		private class internalState : AgentState
		{
			public int AdherenceInt { set { base.Adherence = (Interfaces.Domain.Adherence?) value; } }
		}

		private static string SelectAgentState =
@"SELECT
PersonId, 
BatchId,
SourceId,
PlatformTypeId,
BusinessUnitId,
TeamId,
SiteId,
ReceivedTime,
StateCode,
StateGroupId,
StateStartTime,
ActivityId,
NextActivityId,
NextActivityStartTime,
RuleId,
RuleStartTime,
StaffingEffect,
Adherence AS AdherenceInt,
AlarmStartTime
FROM [dbo].[AgentState] ";
	}
}