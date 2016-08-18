using System;
using System.Collections.Generic;
using System.Linq;
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

			setInvalido(true);
		}

		private void setInvalido(bool value)
		{
			var updated = _unitOfWork.Current()
				.CreateSqlQuery($"UPDATE [ReadModel].[KeyValueStore] SET [Value] = '{value}' WHERE [Key] = 'RuleMappingsInvalido'")
				.ExecuteUpdate();
			if (updated == 0)
				_unitOfWork.Current()
					.CreateSqlQuery($"INSERT INTO [ReadModel].[KeyValueStore] ([Key], [Value]) VALUES ('RuleMappingsInvalido', '{value}')")
					.ExecuteUpdate();
		}

		public bool Invalid()
		{
			var value = _unitOfWork.Current()
				.CreateSqlQuery("SELECT [Value] FROM [ReadModel].[KeyValueStore] WHERE [Key] = 'RuleMappingsInvalido'")
				.UniqueResult<string>() ?? bool.TrueString;

			return bool.Parse(value);
		}

		public void Persist(IEnumerable<Mapping> mappings)
		{
			setInvalido(false);

			mappings.Batch(100).ForEach(batch =>
			{

				var sqlValues = batch.Select((m, i) =>
						$@"
(
:BusinessUnitId{i},
:StateCode{i},
:PlatformTypeId{i},
:StateGroupId{i},
:StateGroupName{i},
:ActivityId{i},
:RuleId{i},
:RuleName{i},
:Adherence{i},
:StaffingEffect{i},
:DisplayColor{i},
:IsAlarm{i},
:ThresholdTime{i},
:AlarmColor{i}
)").Aggregate((current, next) => current + ", " + next);
				
				var query = _unitOfWork.Current()
				.CreateSqlQuery($@"
MERGE INTO [ReadModel].[RuleMappings] AS T
USING
(
	VALUES {sqlValues}
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

				batch.Select((m, i) => new {m, i})
					.ForEach(x =>
					{
						var i = x.i;
						var mapping = x.m;
						query
							.SetParameter("BusinessUnitId" + i, mapping.BusinessUnitId)
							.SetParameter("StateCode" + i, mapping.StateCode ?? MappingReadModelReader.MagicString)
							.SetParameter("PlatformTypeId" + i, mapping.PlatformTypeId)
							.SetParameter("StateGroupId" + i, mapping.StateGroupId)
							.SetParameter("StateGroupName" + i, mapping.StateGroupName)
							.SetParameter("ActivityId" + i, mapping.ActivityId ?? Guid.Empty)
							.SetParameter("RuleId" + i, mapping.RuleId)
							.SetParameter("RuleName" + i, mapping.RuleName)
							.SetParameter("Adherence" + i, mapping.Adherence)
							.SetParameter("StaffingEffect" + i, mapping.StaffingEffect)
							.SetParameter("DisplayColor" + i, mapping.DisplayColor)
							.SetParameter("IsAlarm" + i, mapping.IsAlarm)
							.SetParameter("ThresholdTime" + i, mapping.ThresholdTime)
							.SetParameter("AlarmColor" + i, mapping.AlarmColor);
					});

				query.ExecuteUpdate();
			});

			
			_unitOfWork.Current()
				.CreateSqlQuery("DELETE FROM [ReadModel].[RuleMappings] WHERE Updated = 0")
				.ExecuteUpdate();
			_unitOfWork.Current()
				.CreateSqlQuery("UPDATE [ReadModel].[RuleMappings] SET Updated = 0")
				.ExecuteUpdate();

			var updated = _unitOfWork.Current()
				.CreateSqlQuery("UPDATE [ReadModel].[KeyValueStore] SET [Value] = NEWID() WHERE [Key] = 'RuleMappingsVersion'")
				.ExecuteUpdate();
			if (updated == 0)
				_unitOfWork.Current()
					.CreateSqlQuery("INSERT INTO [ReadModel].[KeyValueStore] ([Key], [Value]) VALUES ('RuleMappingsVersion', NEWID())")
					.ExecuteUpdate();
		}
	}
}