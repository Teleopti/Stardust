using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelPersister : IAgentStateReadModelPersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;
		private readonly IJsonDeserializer _deserializer;

		public AgentStateReadModelPersister(ICurrentUnitOfWork unitOfWork, IJsonSerializer serializer, IJsonDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
		}

		[LogInfo]
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
						Shift = :Shift,
						OutOfAdherences = :OutOfAdherences,
						StateGroupId = :StateGroupId
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
				.SetParameter("Shift", _serializer.SerializeObject(model.Shift), NHibernateUtil.StringClob)
				.SetParameter("OutOfAdherences", _serializer.SerializeObject(model.OutOfAdherences), NHibernateUtil.StringClob)
				.SetParameter("StateGroupId", model.StateGroupId)
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
							Shift,
							OutOfAdherences,
							StateGroupId
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
							:Shift,
							:OutOfAdherences,
							:StateGroupId
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
					.SetParameter("Shift", _serializer.SerializeObject(model.Shift), NHibernateUtil.StringClob)
					.SetParameter("OutOfAdherences", _serializer.SerializeObject(model.OutOfAdherences), NHibernateUtil.StringClob)
					.SetParameter("StateGroupId", model.StateGroupId)
					.ExecuteUpdate();
			}
		}

		public virtual void SetDeleted(Guid personId, DateTime expiresAt)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(
					@"UPDATE [ReadModel].[AgentState] SET IsDeleted=1, ExpiresAt = :ExpiresAt WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.SetParameter("ExpiresAt", expiresAt)
				.ExecuteUpdate()
				;
		}

		public void DeleteOldRows(DateTime now)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(
					@"DELETE FROM [ReadModel].[AgentState] WHERE ExpiresAt <= :now")
				.SetParameter("now", now)
				.ExecuteUpdate()
				;
		}

		public AgentStateReadModel Get(Guid personId)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"SELECT * FROM [ReadModel].[AgentState] WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.SetReadOnly(true)
				.List<internalModel>()
				.Select(x =>
				{
					(x as AgentStateReadModel).Shift = _deserializer.DeserializeObject<AgentStateActivityReadModel[]>(x.Shift);
					x.Shift = null;
					(x as AgentStateReadModel).OutOfAdherences = _deserializer.DeserializeObject<AgentStateOutOfAdherenceReadModel[]>(x.OutOfAdherences);
					x.OutOfAdherences = null;
					return x;
				})
				.FirstOrDefault();
		}

		public void UpdateAssociation(Guid personId, Guid teamId, Guid? siteId)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"
					UPDATE [ReadModel].[AgentState]
					SET
						SiteId = :SiteId,
						TeamId = :TeamId
					WHERE 
						PersonId = :PersonId
				")
				.SetParameter("PersonId", personId)
				.SetParameter("SiteId", siteId)
				.SetParameter("TeamId", teamId)
				.ExecuteUpdate();
		}

		private class internalModel : AgentStateReadModel
		{
			public new string Shift { get; set; }
			public new string OutOfAdherences{ get; set; }
		}
	}
}