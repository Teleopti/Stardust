using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Wfm.Adherence.States.Infrastructure
{
	public class MappingReadModelPersister : IMappingReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;
		private readonly IConfigReader _config;
		private readonly IKeyValueStorePersister _keyValueStore;

		public MappingReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork, IConfigReader config, IKeyValueStorePersister keyValueStore)
		{
			_unitOfWork = unitOfWork;
			_config = config;
			_keyValueStore = keyValueStore;
		}

		public void Invalidate()
		{
			if (Invalid())
				return;
			setInvalido(true);
		}

		private void setInvalido(bool value)
		{
			_keyValueStore.Update("RuleMappingsInvalido", value);
		}

		public bool Invalid()
		{
			return Ccc.Domain.ApplicationLayer.KeyValueStorePersisterExtensions.Get(_keyValueStore, "RuleMappingsInvalido", true);
		}

		public void Persist(IEnumerable<Mapping> mappings)
		{
			setInvalido(false);

			var batchSize = _config.ReadValue("MappingReadModelPersisterBatchSize", 20);

			mappings.Batch(batchSize).ForEach(batch =>
			{

				var sqlValues = batch.Select((m, i) =>
						$@"
(
:BusinessUnitId{i},
:StateCode{i},
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
:AlarmColor{i},
:IsLoggedOut{i}
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
	IsLoggedOut
) ON
	T.BusinessUnitId = S.BusinessUnitId
	AND T.StateCode = S.StateCode
	AND T.ActivityId = S.ActivityId
WHEN NOT MATCHED THEN
INSERT
(
	BusinessUnitId,
	StateCode,
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
	IsLoggedOut,
	Updated
)
VALUES (
	S.BusinessUnitId,
	S.StateCode,
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
	S.IsLoggedOut,
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
	IsLoggedOut = S.IsLoggedOut,
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
							.SetParameter("AlarmColor" + i, mapping.AlarmColor)
							.SetParameter("IsLoggedOut" + i, mapping.IsLoggedOut);
					});

				query.ExecuteUpdate();
			});
			
			_unitOfWork.Current()
				.CreateSqlQuery("DELETE FROM [ReadModel].[RuleMappings] WHERE Updated = 0")
				.ExecuteUpdate();
			_unitOfWork.Current()
				.CreateSqlQuery("UPDATE [ReadModel].[RuleMappings] SET Updated = 0")
				.ExecuteUpdate();

			_keyValueStore.Update("RuleMappingsVersion", Guid.NewGuid().ToString());
		}
	}
}