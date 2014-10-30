using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class ForecastWorkloadTransformer : IEtlTransformer<ISkillDay>
    {
        private readonly int _intervalsPerDay;
        private readonly DateTime _insertDateTime;

        private ForecastWorkloadTransformer()
        {
        }

        public ForecastWorkloadTransformer(int intervalsPerDay, DateTime insertDateTime)
            : this()
        {
            _intervalsPerDay = intervalsPerDay;
            _insertDateTime = insertDateTime;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void Transform(IEnumerable<ISkillDay> rootList, DataTable table)
        {
            InParameter.NotNull("rootList", rootList);

            int minutesPerInterval = 1440 / _intervalsPerDay;
            foreach (ISkillDay skillDay in rootList)
            {
                ReadOnlyCollection<IWorkloadDay> workloadDayCollection = skillDay.WorkloadDayCollection;
                foreach (IWorkloadDay workloadDay in workloadDayCollection)
                {
                    ReadOnlyCollection<ITemplateTaskPeriodView> templateTaskPeriodList = workloadDay.TemplateTaskPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval));
                    foreach (ITemplateTaskPeriodView taskPeriod in templateTaskPeriodList)
                    {
                        AddDataRowToDataTable(taskPeriod, skillDay.Skill, table);
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void AddDataRowToDataTable(ITemplateTaskPeriodView templateTaskPeriod, ISkill skill, DataTable table)
        {
            InParameter.NotNull("templateTaskPeriod", templateTaskPeriod);
            InParameter.NotNull("skill", skill);
            InParameter.NotNull("table", table);

            IWorkloadDay workloadDay = templateTaskPeriod.Parent;
            ISkillDay skillDay = (ISkillDay)workloadDay.Parent;

            DataRow dataRow = table.NewRow();

            dataRow["date"] = templateTaskPeriod.Period.StartDateTime.Date;
            dataRow["interval_id"] = new Interval(templateTaskPeriod.Period.StartDateTime, _intervalsPerDay).Id;
            dataRow["start_time"] = templateTaskPeriod.Period.StartDateTime;
            dataRow["workload_code"] = workloadDay.Workload.Id;
            dataRow["scenario_code"] = skillDay.Scenario.Id;
            dataRow["end_time"] = templateTaskPeriod.Period.EndDateTime;
            dataRow["skill_code"] = skill.Id;

            dataRow["forecasted_calls"] = templateTaskPeriod.TotalTasks; //Both tasks and campaign tasks
            dataRow["forecasted_campaign_calls"] = templateTaskPeriod.CampaignTasks; //Only campaign
            dataRow["forecasted_calls_excl_campaign"] = templateTaskPeriod.Tasks; //Only tasks
            dataRow["forecasted_talk_time_sec"] = templateTaskPeriod.TotalAverageTaskTime.TotalSeconds *
                                                  templateTaskPeriod.TotalTasks; //Both tasks and campaign tasks
            dataRow["forecasted_campaign_talk_time_s"] = templateTaskPeriod.CampaignTaskTime.Value *
                                                         (double)dataRow["forecasted_talk_time_sec"];
            //Only campaign
            dataRow["forecasted_talk_time_excl_campaign_s"] = templateTaskPeriod.AverageTaskTime.TotalSeconds *
                                                              (double)dataRow["forecasted_calls_excl_campaign"];
            //Only tasks
            dataRow["forecasted_after_call_work_s"] = templateTaskPeriod.TotalAverageAfterTaskTime.TotalSeconds *
                                                      templateTaskPeriod.TotalTasks;
            //Both tasks and campaign tasks
            dataRow["forecasted_campaign_after_call_work_s"] = templateTaskPeriod.CampaignAfterTaskTime.Value *
                                                               (double)dataRow["forecasted_after_call_work_s"];
            //Only campaign
            dataRow["forecasted_after_call_work_excl_campaign_s"] =
                templateTaskPeriod.AverageAfterTaskTime.TotalSeconds *
                (double)dataRow["forecasted_calls_excl_campaign"]; //Only tasks
            dataRow["forecasted_handling_time_s"] = (double)dataRow["forecasted_talk_time_sec"] +
                                                    (double)dataRow["forecasted_after_call_work_s"];
            //Both tasks and campaign tasks
            dataRow["forecasted_campaign_handling_time_s"] =
                (double)dataRow["forecasted_campaign_talk_time_s"] +
                (double)dataRow["forecasted_campaign_after_call_work_s"]; //Only campaign
            dataRow["forecasted_handling_time_excl_campaign_s"] =
                (double)dataRow["forecasted_talk_time_excl_campaign_s"] +
                (double)dataRow["forecasted_after_call_work_excl_campaign_s"]; //Only tasks


            dataRow["period_length_min"] =
                templateTaskPeriod.Period.EndDateTime.Subtract(templateTaskPeriod.Period.StartDateTime).TotalMinutes;

            dataRow["business_unit_code"] = workloadDay.Workload.Skill.BusinessUnit.Id;
            dataRow["business_unit_name"] = workloadDay.Workload.Skill.BusinessUnit.Name;
            dataRow["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
            dataRow["insert_date"] = _insertDateTime;
            dataRow["update_date"] = _insertDateTime;
			dataRow["datasource_update_date"] = skillDay.UpdatedOn;

            switch (skill.SkillType.ForecastSource)
            {
                case ForecastSource.Email:
                    dataRow["forecasted_emails"] = templateTaskPeriod.Tasks;
                    //dataRow["forecasted_handling_time_s"] = (templateTaskPeriod.AverageTaskTime.TotalSeconds*
                    //                                         templateTaskPeriod.Tasks) +
                    //                                        (templateTaskPeriod.AverageAfterTaskTime.TotalSeconds*
                    //                                         templateTaskPeriod.Tasks);
                    break;
                case ForecastSource.Backoffice:
                    dataRow["forecasted_backoffice_tasks"] = templateTaskPeriod.Tasks;
                    //dataRow["forecasted_handling_time_s"] = (templateTaskPeriod.AverageTaskTime.TotalSeconds*
                    //                                         templateTaskPeriod.Tasks) +
                    //                                        (templateTaskPeriod.AverageAfterTaskTime.TotalSeconds*
                    //                                         templateTaskPeriod.Tasks);
                    break;
                default:
                    break;
            }

            table.Rows.Add(dataRow);
        }
    }
}
