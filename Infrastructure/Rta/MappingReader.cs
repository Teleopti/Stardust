using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class MappingReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public MappingReader(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<Mapping> Read()
		{
			return _unitOfWork.Session()
				.CreateSQLQuery(@"
					SELECT
						ISNULL(RtaMap.BusinessUnit, RtaStateGroup.BusinessUnit) AS BusinessUnitId,

						RtaState.StateCode AS StateCode,
						RtaState.PlatformTypeId AS PlatformTypeId,
						RtaStateGroup.Id AS StateGroupId,
						RtaStateGroup.Name AS StateGroupName,
						RtaStateGroup.IsLogOutState AS IsLogOutState,

						RtaMap.Activity AS ActivityId,

						RtaRule.Id AS RuleId,
						RtaRule.Name AS RuleName,
						RtaRule.Adherence AS AdherenceInt,
						RtaRule.StaffingEffect AS StaffingEffect,
						RtaRule.DisplayColor AS DisplayColor,

						RtaRule.IsAlarm AS IsAlarm,
						RtaRule.AlarmColor AS AlarmColor,
						RtaRule.ThresholdTime AS ThresholdTime

					FROM 
						RtaMap
					FULL JOIN RtaStateGroup ON
						RtaStateGroup.Id = RtaMap.StateGroup
					FULL JOIN RtaState ON
						RtaState.Parent = RtaStateGroup.Id
					LEFT JOIN RtaRule ON
						RtaRule.Id = RtaMap.RtaRule
					")
				.SetResultTransformer(Transformers.AliasToBean(typeof (readMapping)))
				.List<readMapping>();
		}

		private class readMapping : Mapping
		{
			public int? AdherenceInt
			{
				set
				{
					if (value.HasValue)
						Adherence = (Adherence) value.Value;
				}
			}
		}
	}
}