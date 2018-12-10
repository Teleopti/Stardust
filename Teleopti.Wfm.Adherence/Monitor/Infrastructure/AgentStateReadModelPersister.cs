using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Monitor.Infrastructure
{
	public class AgentStateReadModelPersister : IAgentStateReadModelPersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;
		private readonly AgentStateReadModelQueryBuilderConfiguration _configuration;

		public AgentStateReadModelPersister(
			ICurrentUnitOfWork unitOfWork,
			IJsonSerializer serializer,
			AgentStateReadModelQueryBuilderConfiguration configuration)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_configuration = configuration;
		}

		[LogInfo]
		public virtual void UpdateState(AgentStateReadModel model)
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
						OutOfAdherenceStartTime = :OutOfAdherenceStartTime,
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
				.SetParameter("OutOfAdherenceStartTime", model.OutOfAdherenceStartTime)
				.SetParameter("StateGroupId", model.StateGroupId)
				.ExecuteUpdate();
		}

		public AgentStateReadModel Load(Guid personId)
		{
			var builder = new AgentStateReadModelQueryBuilder(_configuration)
				.WithPersons(new[] {personId})
				.Build();
			var sqlQuery = _unitOfWork.Current().Session()
				.CreateSQLQuery(builder.Query);
			builder.ParameterFuncs
				.ForEach(f => f(sqlQuery));
			return sqlQuery
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
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
			:TeamName,
			:FirstName,
			:LastName,
			:EmploymentNumber
		)
	) AS S (
			PersonId,
			BusinessUnitId,
			SiteId,
			SiteName,
			TeamId,
			TeamName,
			FirstName,
			LastName,
			EmploymentNumber
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
			FirstName,
			LastName,
			EmploymentNumber,
			HasAssociation
		) VALUES (
			S.PersonId,
			S.BusinessUnitId,
			S.SiteId,
			S.SiteName,
			S.TeamId,
			S.TeamName,
			S.FirstName,
			S.LastName,
			S.EmploymentNumber,
			1
		)
	WHEN MATCHED THEN
		UPDATE SET
			BusinessUnitId = S.BusinessUnitId,
			SiteId = S.SiteId,
			SiteName = S.SiteName,
			TeamId = S.TeamId,
			TeamName = S.TeamName,
			FirstName = S.FirstName,
			LastName = S.LastName,
			EmploymentNumber = S.EmploymentNumber,
			HasAssociation = 1
		;")
				.SetParameter("PersonId", info.PersonId)
				.SetParameter("BusinessUnitId", info.BusinessUnitId)
				.SetParameter("SiteId", info.SiteId)
				.SetParameter("SiteName", info.SiteName)
				.SetParameter("TeamId", info.TeamId)
				.SetParameter("TeamName", info.TeamName)
				.SetParameter("FirstName", info.FirstName)
				.SetParameter("LastName", info.LastName)
				.SetParameter("EmploymentNumber", info.EmploymentNumber)
				.ExecuteUpdate();
		}

		public void UpsertNoAssociation(Guid personId)
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
			HasAssociation
		) VALUES (
			S.PersonId,
			0
		)
	WHEN MATCHED THEN
		UPDATE SET
			HasAssociation = 0
		;")
				.SetParameter("PersonId", personId)
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