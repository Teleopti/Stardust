using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.JobSchedule;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;


namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class JobScheduleRepository : IJobScheduleRepository
	{
		private string _connectionString;

		private string dataMartConnectionString
		{
			get
			{
				if (_connectionString == null)
					throw new ArgumentException("You need to set the datamart connection string before using it.",
						nameof(_connectionString));

				return _connectionString;
			}
			set => _connectionString = value;
		}
		public IList<IEtlJobSchedule> GetSchedules(IEtlJobLogCollection etlJobLogCollection, DateTime serverStartTime)
		{
			var etlScheduleJobs = new List<IEtlJobSchedule>();

			DataSet ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.etl_job_get_schedules", null,
				dataMartConnectionString);

			foreach (DataRow row in ds.Tables[0].Rows)
			{
				var scheduleType = (int)row["schedule_type"];

				switch (scheduleType)
				{
					case 0:
					{
						etlScheduleJobs.Add(new EtlJobSchedule(
							(int) row["schedule_id"],
							(string) row["schedule_name"],
							(bool) row["enabled"],
							(int) row["occurs_daily_at"],
							(string) row["etl_job_name"],
							handleDBNull(row["etl_datasource_id"], -1),
							handleDBNull(row["description"], string.Empty),
							etlJobLogCollection,
							GetSchedulePeriods((int) row["schedule_id"]),
							(string) row["tenant_name"]));
					}
						break;
					case 1:
					{
						etlScheduleJobs.Add(new EtlJobSchedule(
							(int) row["schedule_id"],
							(string) row["schedule_name"],
							(bool) row["enabled"],
							(int) row["occurs_every_minute"],
							(int) row["recurring_starttime"],
							(int) row["recurring_endtime"],
							(string) row["etl_job_name"],
							handleDBNull(row["etl_datasource_id"], -1),
							handleDBNull(row["description"], string.Empty),
							etlJobLogCollection,
							serverStartTime,
							GetSchedulePeriods((int) row["schedule_id"]),
							(string) row["tenant_name"]));
					}
						break;
					case 2:
					{
						etlScheduleJobs.Add(new EtlJobSchedule(
							(int) row["schedule_id"],
							(string) row["schedule_name"],
							(string) row["etl_job_name"],
							(bool) row["enabled"],
							handleDBNull(row["etl_datasource_id"], -1),
							handleDBNull(row["description"], string.Empty),
							(DateTime) row["insert_date"],
							GetSchedulePeriods((int) row["schedule_id"]),
							(string) row["tenant_name"]));
						}
						break;
				}
			}
			return etlScheduleJobs;
		}

		public int SaveSchedule(IEtlJobSchedule jobSchedule)
		{
			var parameterList = new[]
			{
				new SqlParameter("schedule_id", jobSchedule.ScheduleId),
				new SqlParameter("schedule_name", jobSchedule.ScheduleName),
				new SqlParameter("enabled", jobSchedule.Enabled),
				new SqlParameter("schedule_type", jobSchedule.ScheduleType),
				new SqlParameter("occurs_daily_at", jobSchedule.OccursOnceAt),
				new SqlParameter("occurs_every_minute", jobSchedule.OccursEveryMinute),
				new SqlParameter("recurring_starttime", jobSchedule.OccursEveryMinuteStartingAt),
				new SqlParameter("recurring_endtime", jobSchedule.OccursEveryMinuteEndingAt),
				new SqlParameter("etl_job_name", jobSchedule.JobName),
				new SqlParameter("etl_datasource_id", jobSchedule.DataSourceId),
				new SqlParameter("description", jobSchedule.Description),
				new SqlParameter("tenant_name", jobSchedule.TenantName)
			};

			return
				(int)
				HelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.etl_job_save_schedule", parameterList,
					dataMartConnectionString);
		}

		public IList<IEtlJobRelativePeriod> GetSchedulePeriods(int scheduleId)
		{
			IList<IEtlJobRelativePeriod> schedulePeriods = new List<IEtlJobRelativePeriod>();
			var parameterList = new[] { new SqlParameter("schedule_id", scheduleId) };
			var ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.etl_job_get_schedule_periods",
				parameterList,
				dataMartConnectionString);

			if (ds.Tables[0] == null || ds.Tables[0].Rows.Count <= 0) return schedulePeriods;

			foreach (DataRow row in ds.Tables[0].Rows)
			{
				// Create periods
				var minMaxPeriod = new MinMax<int>(((int)row["relative_period_start"]),
					(int)row["relative_period_end"]);
				var jobName = (string)row["job_name"];
				IEtlJobRelativePeriod relativePeriod = new EtlJobRelativePeriod(minMaxPeriod,
					getJobCategory(jobName));
				schedulePeriods.Add(relativePeriod);
			}

			return schedulePeriods;
		}

		public void SaveSchedulePeriods(IEtlJobSchedule etlJobScheduleItem)
		{
			foreach (IEtlJobRelativePeriod relativePeriod in etlJobScheduleItem.RelativePeriodCollection)
			{
				var parameterList = new[]
				{
					new SqlParameter("schedule_id", etlJobScheduleItem.ScheduleId),
					new SqlParameter("job_name", relativePeriod.JobCategoryName),
					new SqlParameter("relative_period_start", relativePeriod.RelativePeriod.Minimum),
					new SqlParameter("relative_period_end", relativePeriod.RelativePeriod.Maximum),
				};


				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_job_save_schedule_period", parameterList,
					dataMartConnectionString);
			}
		}


		public void DeleteSchedule(int scheduleId)
		{
			var parameterList = new[] { new SqlParameter("schedule_id", scheduleId) };

			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_job_delete_schedule", parameterList,
				dataMartConnectionString);
		}

		public void SetDataMartConnectionString(string connectionString)
		{
			dataMartConnectionString = connectionString;
		}

		public void ToggleScheduleJobEnabledState(int scheduleId)
		{
			var parameterList = new[] { new SqlParameter("schedule_id", scheduleId) };

			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_job_schedule_toggle_enable_state", parameterList,
				dataMartConnectionString);
		}
		private static JobCategoryType getJobCategory(string jobName)
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
					throw new ArgumentException("Invalid job name when trying to read jobschedule relative periods.", nameof(jobName));
			}
		}

		private static T handleDBNull<T>(object value, T defaultValue)
		{
			if (value == DBNull.Value)
				return defaultValue;

			return (T)value;
		}
	}
}