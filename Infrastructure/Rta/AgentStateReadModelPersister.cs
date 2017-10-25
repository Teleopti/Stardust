﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class AgentStateReadModelPersister : IAgentStateReadModelPersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;

		public AgentStateReadModelPersister(
			ICurrentUnitOfWork unitOfWork,
			IJsonSerializer serializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
		}

		[LogInfo]
		public virtual void Update(AgentStateReadModel model)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"
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
				.SetParameter("Shift", model.Shift != null ? _serializer.SerializeObject(model.Shift) : null, NHibernateUtil.StringClob)
				.SetParameter("OutOfAdherences", model.OutOfAdherences != null ? _serializer.SerializeObject(model.OutOfAdherences) : null, NHibernateUtil.StringClob)
				.SetParameter("StateGroupId", model.StateGroupId)
				.ExecuteUpdate();
		}

		public virtual void UpsertDeleted(Guid personId)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"
MERGE INTO [ReadModel].[AgentState] AS T
	USING (
		VALUES
		(
			:PersonId
		)
	) AS S (
			PersonId
		)
	ON 
		T.PersonId = S.PersonId
	WHEN NOT MATCHED THEN
		INSERT
		(
			PersonId,
			IsDeleted
		) VALUES (
			S.PersonId,
			1
		)
	WHEN MATCHED THEN
		UPDATE SET
			IsDeleted = 1
		;")
				.SetParameter("PersonId", personId)
				.ExecuteUpdate();
		}

		public AgentStateReadModel Load(Guid personId)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT TOP 1 * FROM [ReadModel].[AgentState] WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean<internalModel>())
				.SetReadOnly(true)
				.UniqueResult<AgentStateReadModel>();
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
			TeamName,
			IsDeleted
		) VALUES (
			S.PersonId,
			S.BusinessUnitId,
			S.SiteId,
			S.SiteName,
			S.TeamId,
			S.TeamName,
			0
		)
	WHEN MATCHED THEN
		UPDATE SET
			BusinessUnitId = S.BusinessUnitId,
			SiteId = S.SiteId,
			SiteName = S.SiteName,
			TeamId = S.TeamId,
			TeamName = S.TeamName,
			IsDeleted = 0
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
			_unitOfWork.Current()
				.Session().CreateSQLQuery(@"
MERGE INTO [ReadModel].[AgentState] AS T
	USING (
		VALUES
		(
			:PersonId,
			:EmploymentNumber
		)
	) AS S (
			PersonId,
			EmploymentNumber
		)
	ON 
		T.PersonId = S.PersonId
	WHEN NOT MATCHED THEN
		INSERT
		(
			PersonId,
			EmploymentNumber,
			IsDeleted
		) VALUES (
			S.PersonId,
			S.EmploymentNumber,
			1
		)
	WHEN MATCHED THEN
		UPDATE SET
			EmploymentNumber = S.EmploymentNumber
			
		;")
				.SetParameter("PersonId", personId)
				.SetParameter("EmploymentNumber", employmentNumber)
				.ExecuteUpdate();
		}

		public void UpsertName(Guid personId, string firstName, string lastName)
		{
			_unitOfWork.Current()
				.Session().CreateSQLQuery(@"
MERGE INTO [ReadModel].[AgentState] AS T
	USING (
		VALUES
		(
			:PersonId,
			:FirstName,
			:LastName
		)
	) AS S (
			PersonId,
			FirstName,
			LastName
		)
	ON 
		T.PersonId = S.PersonId
	WHEN NOT MATCHED THEN
		INSERT
		(
			PersonId,
			FirstName,
			LastName,
			IsDeleted
		) VALUES (
			S.PersonId,
			S.FirstName,
			S.LastName,
			1
		)
	WHEN MATCHED THEN
		UPDATE SET
			FirstName = S.FirstName,
			LastName = S.LastName
			
		;")
				.SetParameter("PersonId", personId)
				.SetParameter("FirstName", firstName)
				.SetParameter("LastName", lastName)
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
			public new string Shift
			{
				set { base.Shift = value != null ? JsonConvert.DeserializeObject<IEnumerable<AgentStateActivityReadModel>>(value) : null; }
			}

			public new string OutOfAdherences
			{
				set { base.OutOfAdherences = value != null ? JsonConvert.DeserializeObject<IEnumerable<AgentStateOutOfAdherenceReadModel>>(value) : null; }
			}
		}
	}
}