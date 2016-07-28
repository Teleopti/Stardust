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
			if (Invalid())
				return;

			_unitOfWork.Current()
				.CreateSqlQuery("INSERT INTO [ReadModel].[KeyValueStore] ([Key]) VALUES ('RuleMappingsInvalido')")
				.ExecuteUpdate();
		}

		public bool Invalid()
		{
			var isInvalido = 1 == _unitOfWork.Current()
				.CreateSqlQuery("SELECT COUNT(1) FROM [ReadModel].[KeyValueStore] WITH (ROWLOCK, XLOCK) WHERE [Key] = 'RuleMappingsInvalido'")
				.UniqueResult<int>();

			if (isInvalido)
				return true;

			return 0 == _unitOfWork.Current()
				.CreateSqlQuery("SELECT COUNT(1) FROM [ReadModel].[RuleMappings] WITH (TABLOCKX)")
				.UniqueResult<int>();
		}

		public void Persist(IEnumerable<Mapping> mappings)
		{
			_unitOfWork.Current()
				.CreateSqlQuery("DELETE FROM [ReadModel].[KeyValueStore] WHERE [Key] = 'RuleMappingsInvalido'")
				.ExecuteUpdate();

			var query = _unitOfWork.Current()
				.CreateSqlQuery(@"
MERGE INTO [ReadModel].[RuleMappings] AS T
USING (
	VALUES (
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
	)
) AS S (
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
) ON
	T.BusinessUnitId = S.BusinessUnitId
	AND T.StateCode = S.StateCode
	AND T.PlatformTypeId = S.PlatformTypeId
	AND T.ActivityId = S.ActivityId
WHEN NOT MATCHED THEN
INSERT
(
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
	AlarmColor,
	Updated
)
VALUES (
	S.BusinessUnitId,
	S.StateCode,
	S.PlatformTypeId,
	S.StateGroupId,
	S.StateGroupName,
	S.ActivityId,
	S.RuleId,
	S.RuleName,
	S.Adherence,
	S.StaffingEffect,
	S.DisplayColor,
	S.IsAlarm,
	S.ThresholdTime,
	S.AlarmColor,
	1
)
WHEN MATCHED THEN
UPDATE SET
	StateGroupId = S.StateGroupId,
	StateGroupName = S.StateGroupName,
	RuleId = S.RuleId,
	RuleName = S.RuleName,
	Adherence = S.Adherence,
	StaffingEffect = S.StaffingEffect,
	DisplayColor = S.DisplayColor,
	IsAlarm = S.IsAlarm,
	ThresholdTime = S.ThresholdTime,
	AlarmColor = S.AlarmColor,
	Updated = 1
			;");

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

			_unitOfWork.Current()
				.CreateSqlQuery("DELETE FROM [ReadModel].[RuleMappings] WHERE Updated = 0")
				.ExecuteUpdate();
			_unitOfWork.Current()
				.CreateSqlQuery("UPDATE [ReadModel].[RuleMappings] SET Updated = 0")
				.ExecuteUpdate();

			var updated = _unitOfWork.Current()
				.CreateSqlQuery("UPDATE [ReadModel].[KeyValueStore] SET [Value] = [Value] + 1 WHERE [Key] = 'RuleMappingsVersion'")
				.ExecuteUpdate();
			if (updated == 0)
				_unitOfWork.Current()
					.CreateSqlQuery("INSERT INTO [ReadModel].[KeyValueStore] ([Key], [Value]) VALUES ('RuleMappingsVersion', 1)")
					.ExecuteUpdate();
		}
	}
}