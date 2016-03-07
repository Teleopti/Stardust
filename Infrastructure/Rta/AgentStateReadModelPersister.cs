using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelPersister : IAgentStateReadModelPersister
	{
		private readonly ICurrentAnalyticsUnitOfWork _unitOfWork;

		public AgentStateReadModelPersister(ICurrentAnalyticsUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		[InfoLog]
		[AnalyticsUnitOfWork]
		public virtual void PersistActualAgentReadModel(AgentStateReadModel model)
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
				.SetParameter("OriginalDataSourceId", model.OriginalDataSourceId)
				.SetParameter("PlatformTypeId", model.PlatformTypeId)
				.SetParameter("ReceivedTime", model.ReceivedTime)
				.SetParameter("Scheduled", model.Scheduled)
				.SetParameter("ScheduledId", model.ScheduledId)
				.SetParameter("ScheduledNext", model.ScheduledNext)
				.SetParameter("ScheduledNextId", model.ScheduledNextId)
				.SetParameter("NextStart", model.NextStart)
				.SetParameter("StateCode", model.StateCode)
				.SetParameter("State", model.StateName)
				.SetParameter("StateId", model.StateId)
				.SetParameter("StateStartTime", model.StateStartTime)
				.SetParameter("AlarmId", model.RuleId)
				.SetParameter("AlarmName", model.RuleName)
				.SetParameter("RuleStartTime", model.RuleStartTime)
				.SetParameter("Color", model.RuleColor)
				.SetParameter("StaffingEffect", model.StaffingEffect)
				.SetParameter("Adherence", model.Adherence)
				.SetParameter("IsRuleAlarm", model.IsAlarm)
				.SetParameter("AlarmStartTime", model.AlarmStartTime)
				.SetParameter("AlarmColor", model.AlarmColor)
				.ExecuteUpdate();
		}

		[AnalyticsUnitOfWork]
		public virtual void Delete(Guid personId)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(
					@"DELETE RTA.ActualAgentState WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.ExecuteUpdate()
				;
		}
	}
}