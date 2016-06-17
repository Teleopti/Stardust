﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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

		public AgentStateReadModelPersister(ICurrentUnitOfWork unitOfWork, IJsonSerializer serializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
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
						Shift = :Shift,
						OutOfAdherences = :OutOfAdherences
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
				.SetParameter("Shift", _serializer.SerializeObject(model.Shift))
				.SetParameter("OutOfAdherences", _serializer.SerializeObject(model.OutOfAdherences))
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
							OutOfAdherences
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
							:OutOfAdherences
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
					.SetParameter("Shift", _serializer.SerializeObject(model.Shift))
					.SetParameter("OutOfAdherences", _serializer.SerializeObject(model.OutOfAdherences))
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

		public AgentStateReadModel Get(Guid personId)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"SELECT * FROM [ReadModel].[AgentState] WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.SetReadOnly(true)
				.List<AgentStateReadModel>()
				.FirstOrDefault();
		}

		private class internalModel : AgentStateReadModel
		{
			public new string Shift
			{
				set { base.Shift = JsonConvert.DeserializeObject<IEnumerable<AgentStateActivityReadModel>>(value); }
			}

			public new string OutOfAdherences
			{
				set { base.OutOfAdherences = JsonConvert.DeserializeObject<IEnumerable<AgentStateOutOfAdherenceReadModel>>(value); }
			}
		}
	}
}