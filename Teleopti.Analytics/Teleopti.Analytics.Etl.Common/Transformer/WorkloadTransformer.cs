using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class WorkloadTransformer
	{
		private readonly DateTime _insertDateTime;

		private WorkloadTransformer() { }

		public WorkloadTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			 MessageId = "1"),
		 System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			 MessageId = "0")]
		public void Transform(IList<IWorkload> rootList, DataTable workloadTable, DataTable workloadQueueTable)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("workloadTable", workloadTable);
			InParameter.NotNull("workloadQueueTable", workloadQueueTable);

			foreach (var workload in rootList)
			{
				var dataRow = workloadTable.NewRow();

				dataRow["workload_code"] = workload.Id;
				dataRow["workload_name"] = workload.Name;
				dataRow["skill_code"] = workload.Skill.Id;
				dataRow["skill_name"] = workload.Skill.Name;
				dataRow["time_zone_code"] = workload.Skill.TimeZone.Id;
				dataRow["forecast_method_code"] = workload.Skill.SkillType.Id;
				dataRow["forecast_method_name"] = workload.Skill.SkillType.ForecastSource.ToString();
				dataRow["business_unit_code"] = workload.GetOrFillWithBusinessUnit_DONTUSE().Id;
				dataRow["business_unit_name"] = workload.GetOrFillWithBusinessUnit_DONTUSE().Name;

				var adjustments = workload.QueueAdjustments;
				dataRow["percentage_offered"] = adjustments.OfferedTasks.Value;
				dataRow["percentage_overflow_in"] = adjustments.OverflowIn.Value;
				dataRow["percentage_overflow_out"] = adjustments.OverflowOut.Value;
				dataRow["percentage_abandoned"] = adjustments.Abandoned.Value;
				dataRow["percentage_abandoned_short"] = adjustments.AbandonedShort.Value;
				dataRow["percentage_abandoned_within_service_level"] = adjustments.AbandonedWithinServiceLevel.Value;
				dataRow["percentage_abandoned_after_service_level"] = adjustments.AbandonedAfterServiceLevel.Value;

				dataRow["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
				dataRow["insert_date"] = _insertDateTime;
				dataRow["update_date"] = _insertDateTime;
				dataRow["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(workload);

				var workloadIsDeleted = workload is IDeleteTag workloadDeleteCheck && workloadDeleteCheck.IsDeleted;
				dataRow["is_deleted"] = workloadIsDeleted;

				// Create stage bridge only if workload is not deleted (Refer to bug #46863)
				if (!workloadIsDeleted)
				{
					queueSource(workload, workloadQueueTable);
				}

				if (workload.Skill is IDeleteTag skillDeleteCheck)
				{
					dataRow["skill_is_deleted"] = skillDeleteCheck.IsDeleted;
				}

				workloadTable.Rows.Add(dataRow);
			}
		}

		private static void queueSource(IWorkload workload, DataTable workloadQueueTable)
		{
			foreach (var queueSource in workload.QueueSourceCollection)
			{
				var dataRow2 = workloadQueueTable.NewRow();

				dataRow2["queue_code"] = queueSource.QueueOriginalId;
				dataRow2["workload_code"] = workload.Id;
				dataRow2["log_object_data_source_id"] = queueSource.DataSourceId;
				dataRow2["log_object_name"] = queueSource.LogObjectName;

				dataRow2["business_unit_code"] = workload.GetOrFillWithBusinessUnit_DONTUSE().Id;
				dataRow2["business_unit_name"] = workload.GetOrFillWithBusinessUnit_DONTUSE().Name;
				dataRow2["datasource_id"] = 1;

				workloadQueueTable.Rows.Add(dataRow2);
			}
		}
	}
}
