using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelPersister : IAgentStateReadModelPersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public AgentStateReadModelPersister(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		[InfoLog]
		public virtual void Persist(AgentStateReadModel model)
		{
			var updated = _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
					UPDATE [ReadModel].[AgentState]
					SET
						PersonId = :PersonId,
						BusinessUnitId = :BusinessUnitId,
						SiteId = :SiteId,
						TeamId = :TeamId,
						ReceivedTime = :ReceivedTime,
						Activity = :Activity,
						NextActivity = :NextActivity, 
						NextActivityStartTime = :NextActivityStartTime, 
						StateCode = :StateCode, 
						StateName = :StateName, 
						StateStartTime = :StateStartTime, 
						RuleName = :RuleName, 
						RuleStartTime = :RuleStartTime, 
						RuleColor = :RuleColor, 
						StaffingEffect = :StaffingEffect, 
						IsRuleAlarm = :IsRuleAlarm, 
						AlarmStartTime = :AlarmStartTime, 
						AlarmColor = :AlarmColor,
						Shift = :Shift
					WHERE 
						PersonId = :PersonId
				")
				.SetParameter("PersonId", model.PersonId)
				.SetParameter("BusinessUnitId", model.BusinessUnitId)
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("TeamId", model.TeamId)
				.SetParameter("ReceivedTime", model.ReceivedTime)
				.SetParameter("Activity", model.Activity)
				.SetParameter("NextActivity", model.NextActivity)
				.SetParameter("NextActivityStartTime", model.NextActivityStartTime)
				.SetParameter("StateCode", model.StateCode)
				.SetParameter("StateName", model.StateName)
				.SetParameter("StateStartTime", model.StateStartTime)
				.SetParameter("RuleName", model.RuleName)
				.SetParameter("RuleStartTime", model.RuleStartTime)
				.SetParameter("RuleColor", model.RuleColor)
				.SetParameter("StaffingEffect", model.StaffingEffect)
				.SetParameter("IsRuleAlarm", model.IsRuleAlarm)
				.SetParameter("AlarmStartTime", model.AlarmStartTime)
				.SetParameter("AlarmColor", model.AlarmColor)
				.SetParameter("Shift", model.Shift)
				.ExecuteUpdate();
			if (updated == 0)
			{
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
						INSERT INTO [ReadModel].[AgentState]
						(
							PersonId,
							BusinessUnitId,
							SiteId,
							TeamId,
							ReceivedTime,
							Activity,
							NextActivity, 
							NextActivityStartTime, 
							StateCode, 
							StateName, 
							StateStartTime, 
							RuleName, 
							RuleStartTime, 
							RuleColor, 
							StaffingEffect, 
							IsRuleAlarm, 
							AlarmStartTime, 
							AlarmColor,
							Shift
						)
						VALUES
						(
							:PersonId,
							:BusinessUnitId,
							:SiteId,
							:TeamId,
							:ReceivedTime,
							:Activity,
							:NextActivity, 
							:NextActivityStartTime, 
							:StateCode, 
							:StateName, 
							:StateStartTime, 
							:RuleName, 
							:RuleStartTime, 
							:RuleColor, 
							:StaffingEffect, 
							:IsRuleAlarm, 
							:AlarmStartTime, 
							:AlarmColor,
							:Shift
						)
					")
					.SetParameter("PersonId", model.PersonId)
					.SetParameter("BusinessUnitId", model.BusinessUnitId)
					.SetParameter("SiteId", model.SiteId)
					.SetParameter("TeamId", model.TeamId)
					.SetParameter("ReceivedTime", model.ReceivedTime)
					.SetParameter("Activity", model.Activity)
					.SetParameter("NextActivity", model.NextActivity)
					.SetParameter("NextActivityStartTime", model.NextActivityStartTime)
					.SetParameter("StateCode", model.StateCode)
					.SetParameter("StateName", model.StateName)
					.SetParameter("StateStartTime", model.StateStartTime)
					.SetParameter("RuleName", model.RuleName)
					.SetParameter("RuleStartTime", model.RuleStartTime)
					.SetParameter("RuleColor", model.RuleColor)
					.SetParameter("StaffingEffect", model.StaffingEffect)
					.SetParameter("IsRuleAlarm", model.IsRuleAlarm)
					.SetParameter("AlarmStartTime", model.AlarmStartTime)
					.SetParameter("AlarmColor", model.AlarmColor)
					.SetParameter("Shift", model.Shift)
					.ExecuteUpdate();
			}
		}

		public virtual void Delete(Guid personId)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(
					@"DELETE [ReadModel].[AgentState] WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.ExecuteUpdate()
				;
		}
	}
}