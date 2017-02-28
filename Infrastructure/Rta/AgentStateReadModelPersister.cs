﻿using System;
using System.Threading;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
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

		public AgentStateReadModelPersister(
			ICurrentUnitOfWork unitOfWork,
			IJsonSerializer serializer,
			IJsonDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
		}

		[LogInfo]
		public virtual void Persist(AgentStateReadModel model)
		{
			var query = _unitOfWork.Current()
				.Session().CreateSQLQuery(@"
					UPDATE [ReadModel].[AgentState]
					SET
						ReceivedTime = :ReceivedTime,
						Activity = :Activity,
						NextActivity = :NextActivity, 
						NextActivityStartTime = :NextActivityStartTime,  
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

			var updated = query.ExecuteUpdate();

			if (updated == 0)
			{
				_unitOfWork.Current()
					.Session().CreateSQLQuery(@"
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

		public virtual void UpsertDeleted(Guid personId, DateTime expiresAt)
		{
			_unitOfWork.Current()
				.Session().CreateSQLQuery(@"
MERGE INTO [ReadModel].[AgentState] AS T
	USING (
		VALUES
		(
			:PersonId,
			:ExpiresAt
		)
	) AS S (
			PersonId,
			ExpiresAt
		)
	ON 
		T.PersonId = S.PersonId
	WHEN NOT MATCHED THEN
		INSERT
		(
			PersonId,
			ExpiresAt,
			IsDeleted
		) VALUES (
			S.PersonId,
			S.ExpiresAt,
			1
		)
	WHEN MATCHED THEN
		UPDATE SET
			ExpiresAt = S.ExpiresAt,
			IsDeleted = 1
		;")
				.SetParameter("PersonId", personId)
				.SetParameter("ExpiresAt", expiresAt)
				.ExecuteUpdate();
		}

		public void DeleteOldRows(DateTime now)
		{
			while (_unitOfWork.Current()
				.Session().CreateSQLQuery(
					@"DELETE TOP (100) FROM [ReadModel].[AgentState] WHERE ExpiresAt <= :now")
				.SetParameter("now", now)
				.ExecuteUpdate() > 0)
			{
				Thread.Sleep(100);
			}
		}

		public AgentStateReadModel Load(Guid personId)
		{
			var internalModel = _unitOfWork.Current()
				.Session().CreateSQLQuery(@"SELECT TOP 1 * FROM [ReadModel].[AgentState] WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.SetReadOnly(true)
				.UniqueResult<internalModel>();
			if (internalModel == null) return null;

			((AgentStateReadModel) internalModel).Shift =
				_deserializer.DeserializeObject<AgentStateActivityReadModel[]>(internalModel.Shift);
			internalModel.Shift = null;
			((AgentStateReadModel) internalModel).OutOfAdherences =
				_deserializer.DeserializeObject<AgentStateOutOfAdherenceReadModel[]>(internalModel.OutOfAdherences);
			internalModel.OutOfAdherences = null;

			return internalModel;
		}

		public void UpsertAssociation(AssociationInfo info)
		{
			_unitOfWork.Current()
				.Session().CreateSQLQuery(@"
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

		public void UpsertEmploymentNumber(Guid personId, string employmentNumber, DateTime? expiresAt)
		{
			_unitOfWork.Current()
				.Session().CreateSQLQuery(@"
MERGE INTO [ReadModel].[AgentState] AS T
	USING (
		VALUES
		(
			:PersonId,
			:EmploymentNumber,
			:ExpiresAt
		)
	) AS S (
			PersonId,
			EmploymentNumber,
			ExpiresAt
		)
	ON 
		T.PersonId = S.PersonId
	WHEN NOT MATCHED THEN
		INSERT
		(
			PersonId,
			EmploymentNumber,
			IsDeleted,
			ExpiresAt
		) VALUES (
			S.PersonId,
			S.EmploymentNumber,
			1,
			S.ExpiresAt
		)
	WHEN MATCHED THEN
		UPDATE SET
			EmploymentNumber = S.EmploymentNumber
			
		;")
				.SetParameter("PersonId", personId)
				.SetParameter("EmploymentNumber", employmentNumber)
				.SetParameter("ExpiresAt", expiresAt)
				.ExecuteUpdate();

		}

		public void UpsertName(Guid personId, string firstName, string lastName, DateTime? expiresAt)
		{
			_unitOfWork.Current()
				.Session().CreateSQLQuery(@"
MERGE INTO [ReadModel].[AgentState] AS T
	USING (
		VALUES
		(
			:PersonId,
			:FirstName,
			:LastName,
			:ExpiresAt
		)
	) AS S (
			PersonId,
			FirstName,
			LastName,
			ExpiresAt
		)
	ON 
		T.PersonId = S.PersonId
	WHEN NOT MATCHED THEN
		INSERT
		(
			PersonId,
			FirstName,
			LastName,
			IsDeleted,
			ExpiresAt
		) VALUES (
			S.PersonId,
			S.FirstName,
			S.LastName,
			1,
			S.ExpiresAt
		)
	WHEN MATCHED THEN
		UPDATE SET
			FirstName = S.FirstName,
			LastName = S.LastName
			
		;")
				.SetParameter("PersonId", personId)
				.SetParameter("FirstName", firstName)
				.SetParameter("LastName", lastName)
				.SetParameter("ExpiresAt", expiresAt)
				.ExecuteUpdate();
		}

		public void UpdateTeamName(Guid teamId, string name)
		{
			_unitOfWork.Current()
				.Session().CreateSQLQuery(@"
UPDATE [ReadModel].[AgentState]
SET TeamName = :TeamName
WHERE TeamId = :TeamId")
				.SetParameter("TeamName", name)
				.SetParameter("TeamId", teamId)
				.ExecuteUpdate();
		}

		public void UpdateSiteName(Guid siteId, string name)
		{
			_unitOfWork.Current()
				.Session().CreateSQLQuery(@"
UPDATE [ReadModel].[AgentState]
SET SiteName = :SiteName
WHERE SiteId = :SiteId")
				.SetParameter("SiteName", name)
				.SetParameter("SiteId", siteId)
				.ExecuteUpdate();
		}

		private class internalModel : AgentStateReadModel
		{
			public new string Shift { get; set; }
			public new string OutOfAdherences { get; set; }
		}
	}
}