using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Rta.Persisters
{
	public class MappingReadModelPersister : IMappingReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public MappingReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Invalidate()
		{
			_unitOfWork.Current()
				.CreateSqlQuery("INSERT INTO [ReadModel].[KeyValueStore] ([Key]) VALUES ('RuleMappingsInvalido')")
				.ExecuteUpdate();
		}

		public bool Invalid()
		{
			return 1 == _unitOfWork.Current()
				.CreateSqlQuery("SELECT COUNT(1) FROM [ReadModel].[KeyValueStore] WHERE [Key] = 'RuleMappingsInvalido'")
				.UniqueResult<int>();
		}

		public void Persist(IEnumerable<Mapping> mappings)
		{
			_unitOfWork.Current()
				.CreateSqlQuery("DELETE FROM [ReadModel].[KeyValueStore] WHERE [Key] = 'RuleMappingsInvalido'")
				.ExecuteUpdate();

			_unitOfWork.Current()
				.CreateSqlQuery("DELETE FROM [ReadModel].[RuleMappings]")
				.ExecuteUpdate();

			var query = _unitOfWork.Current()
				.CreateSqlQuery(@"
INSERT INTO [ReadModel].[RuleMappings] (
	BusinessUnitId,
	StateCode,
	PlatformTypeId,
	StateGroupId,
	StateGroupName,
	ActivityId,
	RuleId,
	RuleName,
	Adherence,
	StaffingEffect,
	DisplayColor,
	IsAlarm,
	ThresholdTime,
	AlarmColor
) VALUES (
	:BusinessUnitId,
	:StateCode,
	:PlatformTypeId,
	:StateGroupId,
	:StateGroupName,
	:ActivityId,
	:RuleId,
	:RuleName,
	:Adherence,
	:StaffingEffect,
	:DisplayColor,
	:IsAlarm,
	:ThresholdTime,
	:AlarmColor
)");

			mappings.ForEach(mapping =>
			{
				query
					.SetParameter("BusinessUnitId", mapping.BusinessUnitId)
					.SetParameter("StateCode", mapping.StateCode ?? MappingReadModelReader.MagicString)
					.SetParameter("PlatformTypeId", mapping.PlatformTypeId)
					.SetParameter("StateGroupId", mapping.StateGroupId)
					.SetParameter("StateGroupName", mapping.StateGroupName)
					.SetParameter("ActivityId", mapping.ActivityId ?? Guid.Empty)
					.SetParameter("RuleId", mapping.RuleId)
					.SetParameter("RuleName", mapping.RuleName)
					.SetParameter("Adherence", mapping.Adherence)
					.SetParameter("StaffingEffect", mapping.StaffingEffect)
					.SetParameter("DisplayColor", mapping.DisplayColor)
					.SetParameter("IsAlarm", mapping.IsAlarm)
					.SetParameter("ThresholdTime", mapping.ThresholdTime)
					.SetParameter("AlarmColor", mapping.AlarmColor)
					.ExecuteUpdate();
			});
		}
	}
}