using System;
using System.Linq;
using System.Threading;
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
		private readonly DeadLockVictimThrower _deadLockVictimThrower;

		public AgentStateReadModelPersister(ICurrentUnitOfWork unitOfWork, IJsonSerializer serializer, IJsonDeserializer deserializer, DeadLockVictimThrower deadLockVictimThrower)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
			_deadLockVictimThrower = deadLockVictimThrower;
		}

		[LogInfo]
		public virtual void Persist(AgentStateReadModel model, DeadLockVictim deadLockVictim)
		{
			_deadLockVictimThrower.SetDeadLockPriority(deadLockVictim);

			var query = _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
					UPDATE [ReadModel].[AgentState]
					SET
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
				.SetParameter("StateGroupId", model.StateGroupId);

			var updated = _deadLockVictimThrower.ThrowOnDeadlock(query.ExecuteUpdate);

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
			while (_unitOfWork.Current().Session()
				.CreateSQLQuery(
					@"DELETE TOP (100) FROM [ReadModel].[AgentState] WHERE ExpiresAt <= :now")
				.SetParameter("now", now)
				.ExecuteUpdate() > 0)
			{
				Thread.Sleep(100);
			}
		}

		public AgentStateReadModel Load(Guid personId)
		{
			var internalModel = _unitOfWork.Current().Session()
				.CreateSQLQuery(@"SELECT TOP 1 * FROM [ReadModel].[AgentState] WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.UniqueResult<internalModel>();
			if (internalModel == null) return null;

			((AgentStateReadModel) internalModel).Shift = _deserializer.DeserializeObject<AgentStateActivityReadModel[]>(internalModel.Shift);
			internalModel.Shift = null;
			((AgentStateReadModel) internalModel).OutOfAdherences =
				_deserializer.DeserializeObject<AgentStateOutOfAdherenceReadModel[]>(internalModel.OutOfAdherences);
			internalModel.OutOfAdherences = null;

			return internalModel;
		}

		public void UpsertAssociation(AssociationInfo info)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"
MERGE INTO [ReadModel].[AgentState] AS T
	USING (
		VALUES
		(
			:PersonId,
			:BusinessUnitId,
			:SiteId,
			:SiteName,
			:TeamId,
			:TeamName
		)
	) AS S (
			PersonId,
			BusinessUnitId,
			SiteId,
			SiteName,
			TeamId,
			TeamName
		)
	ON 
		T.PersonId = S.PersonId
	WHEN NOT MATCHED THEN
		INSERT
		(
			PersonId,
			BusinessUnitId,
			SiteId,
			SiteName,
			TeamId,
			TeamName
		) VALUES (
			S.PersonId,
			S.BusinessUnitId,
			S.SiteId,
			S.SiteName,
			S.TeamId,
			S.TeamName
		)
	WHEN MATCHED THEN
		UPDATE SET
			BusinessUnitId = S.BusinessUnitId,
			SiteId = S.SiteId,
			SiteName = S.SiteName,
			TeamId = S.TeamId,
			TeamName = S.TeamName,
			IsDeleted = null,
			ExpiresAt = null
		;")
				.SetParameter("PersonId", info.PersonId)
				.SetParameter("BusinessUnitId", info.BusinessUnitId)
				.SetParameter("SiteId", info.SiteId)
				.SetParameter("SiteName", info.SiteName)
				.SetParameter("TeamId", info.TeamId)
				.SetParameter("TeamName", info.TeamName)
				.ExecuteUpdate();
		}

		public void UpsertEmploymentNumber(Guid personId, string employmentNumber)
		{
			throw new NotImplementedException();
		}

		private class internalModel : AgentStateReadModel
		{
			public new string Shift { get; set; }
			public new string OutOfAdherences { get; set; }
		}
	}
}