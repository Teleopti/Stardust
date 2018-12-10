using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class ScheduleForecastSkillTransformer
	{
		private readonly int _intervalsPerDay;
		private readonly DateTime _insertDateTime;

		private ScheduleForecastSkillTransformer() { }

		public ScheduleForecastSkillTransformer(int intervalsPerDay, DateTime insertDateTime)
			: this()
		{
			_intervalsPerDay = intervalsPerDay;
			_insertDateTime = insertDateTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IScheduleForecastSkillResourceCalculation scheduleForecastSkillResourceCalculation, DataTable table, ISchedulingResultStateHolder schedulingResultStateHolder = null)
		{
			InParameter.NotNull("scheduleForecastSkillResourceCalculation", scheduleForecastSkillResourceCalculation);

			// First get resource calculation data without shrinkage... 
			scheduleForecastSkillResourceCalculation.GetResourceDataExcludingShrinkage(_insertDateTime, schedulingResultStateHolder);
			// ...and then with shrinkage
			Dictionary<IScheduleForecastSkillKey, IScheduleForecastSkill> scheduleForecastSkillDictionary =
				 scheduleForecastSkillResourceCalculation.GetResourceDataIncludingShrinkage(_insertDateTime, schedulingResultStateHolder);

			foreach (var forecastSkill in scheduleForecastSkillDictionary.Values)
			{
				AddRowToDataTable(table, forecastSkill);
			}
		}

		private void AddRowToDataTable(DataTable table, IScheduleForecastSkill forecastSkill)
		{
			DataRow dataRow = table.NewRow();

			dataRow["date"] = forecastSkill.StartDateTime.Date;
			dataRow["interval_id"] = new IntervalBase(forecastSkill.StartDateTime, _intervalsPerDay).Id;
			dataRow["skill_code"] = forecastSkill.SkillCode;
			dataRow["scenario_code"] = forecastSkill.ScenarioCode;
			dataRow["forecasted_resources_m"] = forecastSkill.ForecastedResourcesMinutes;
			dataRow["forecasted_resources"] = forecastSkill.ForecastedResources;
			dataRow["forecasted_resources_incl_shrinkage_m"] = forecastSkill.ForecastedResourcesIncludingShrinkageMinutes;
			dataRow["forecasted_resources_incl_shrinkage"] = forecastSkill.ForecastedResourcesIncludingShrinkage;
			dataRow["scheduled_resources_m"] = forecastSkill.ScheduledResourcesMinutes;
			dataRow["scheduled_resources"] = forecastSkill.ScheduledResources;
			dataRow["scheduled_resources_incl_shrinkage_m"] = forecastSkill.ScheduledResourcesIncludingShrinkageMinutes;     // new
			dataRow["scheduled_resources_incl_shrinkage"] = forecastSkill.ScheduledResourcesIncludingShrinkage;                            // new
			dataRow["forecasted_tasks"] = forecastSkill.ForecastedTasks;
			dataRow["estimated_tasks_answered_within_sl"] = forecastSkill.EstimatedTasksAnsweredWithinSL;
			dataRow["forecasted_tasks_incl_shrinkage"] = forecastSkill.ForecastedTasksIncludingShrinkage;
			dataRow["estimated_tasks_answered_within_sl_incl_shrinkage"] = forecastSkill.EstimatedTasksAnsweredWithinSLIncludingShrinkage;

			dataRow["business_unit_code"] = forecastSkill.BusinessUnitCode;
			dataRow["business_unit_name"] = forecastSkill.BusinessUnitName;
			dataRow["datasource_id"] = forecastSkill.DataSourceId;
			dataRow["insert_date"] = forecastSkill.InsertDate;
			dataRow["update_date"] = forecastSkill.UpdateDate;

			table.Rows.Add(dataRow);
		}
	}
}