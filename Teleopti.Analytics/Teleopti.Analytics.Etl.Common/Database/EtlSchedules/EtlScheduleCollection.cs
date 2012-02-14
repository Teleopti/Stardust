using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Database.EtlSchedules
{
    public class EtlScheduleCollection : List<IEtlSchedule>, IEtlScheduleCollection
    {
        public EtlScheduleCollection(Interfaces.Common.IScheduleRepository repository, IEtlLogCollection etlLogCollection, DateTime serverStartTime)
        {
            Populate(repository, etlLogCollection, serverStartTime);
        }


        public void Populate(Interfaces.Common.IScheduleRepository rep, IEtlLogCollection etlLogCollection, DateTime serverStartTime)
        {

            DataTable dataTable = rep.GetSchedules();

            foreach (DataRow row in dataTable.Rows)
            {
                var scheduleType = (int)row["schedule_type"];


                switch (scheduleType)
                {
                    case 0:
                        {
                            IEtlSchedule etlSchedule = new EtlSchedule((int)row["schedule_id"], (string)row["schedule_name"],
                                                                       (bool)row["enabled"], (int)row["occurs_daily_at"],
                                                                       (string)row["etl_job_name"],
                                                                       row["etl_relative_period_start"] == DBNull.Value ? 0 : (int)row["etl_relative_period_start"],
                                                                       row["etl_relative_period_end"] == DBNull.Value ? 0 : (int)row["etl_relative_period_end"],
                                                                       row["etl_datasource_id"] == DBNull.Value ? -1 : (int)row["etl_datasource_id"],
                                                                       row["description"] == DBNull.Value ? "" : (string)row["description"],
                                                                       etlLogCollection, GetSchedulePeriods((int)row["schedule_id"], rep));
                            Add(etlSchedule);
                        }
                        break;
                    case 1:
                        {
                            IEtlSchedule etlSchedule = new EtlSchedule((int)row["schedule_id"], (string)row["schedule_name"],
                                                                       (bool)row["enabled"], (int)row["occurs_every_minute"],
                                                                       (int)row["recurring_starttime"],
                                                                       (int)row["recurring_endtime"],
                                                                       (string)row["etl_job_name"],
                                                                       row["etl_relative_period_start"] == DBNull.Value ? 0 : (int)row["etl_relative_period_start"],
                                                                       row["etl_relative_period_end"] == DBNull.Value ? 0 : (int)row["etl_relative_period_end"],
                                                                       row["etl_datasource_id"] == DBNull.Value ? -1 : (int)row["etl_datasource_id"],
                                                                       row["description"] == DBNull.Value ? "" : (string)row["description"],
                                                                       etlLogCollection, serverStartTime, GetSchedulePeriods((int)row["schedule_id"], rep));
                            Add(etlSchedule);
                        }
                        break;
                }
            }
        }

        private static IList<IEtlJobRelativePeriod> GetSchedulePeriods(int scheduleId, Interfaces.Common.IScheduleRepository rep)
        {
            IList<IEtlJobRelativePeriod> schedulePeriods = new List<IEtlJobRelativePeriod>();
            DataTable dataTable = rep.GetSchedulePeriods(scheduleId);

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    // Create periods
                    var minMaxPeriod = new MinMax<int>(((int)row["relative_period_start"]),
                                                               (int)row["relative_period_end"]);
                    var jobName = (string)row["job_name"];
                    IEtlJobRelativePeriod relativePeriod = new EtlJobRelativePeriod(minMaxPeriod,
                                                                                    GetJobCategory(jobName));
                    schedulePeriods.Add(relativePeriod);
                }
            }

            return schedulePeriods;
        }

        private static JobCategoryType GetJobCategory(string jobName)
        {
            switch (jobName)
            {
                case "Initial":
                    return JobCategoryType.Initial;
                case "Schedule":
                    return JobCategoryType.Schedule;
                case "Queue Statistics":
                    return JobCategoryType.QueueStatistics;
                case "Forecast":
                    return JobCategoryType.Forecast;
                case "Agent Statistics":
                    return JobCategoryType.AgentStatistics;
                default:
                    throw new ArgumentException("Invalid job name when trying to read jobschedule relative periods.", "jobName");
            }
        }
    }
}