using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
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
		public DataTable GetSchedules()
		{
			DataSet ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.etl_job_get_schedules", null,
				dataMartConnectionString);
			return ds.Tables[0];
		}

		public int SaveSchedule(IEtlJobSchedule etlJobScheduleItem)
		{
			var parameterList = new[]
			{
				new SqlParameter("schedule_id", etlJobScheduleItem.ScheduleId),
				new SqlParameter("schedule_name", etlJobScheduleItem.ScheduleName),
				new SqlParameter("enabled", etlJobScheduleItem.Enabled),
				new SqlParameter("schedule_type", etlJobScheduleItem.ScheduleType),
				new SqlParameter("occurs_daily_at", etlJobScheduleItem.OccursOnceAt),
				new SqlParameter("occurs_every_minute", etlJobScheduleItem.OccursEveryMinute),
				new SqlParameter("recurring_starttime", etlJobScheduleItem.OccursEveryMinuteStartingAt),
				new SqlParameter("recurring_endtime", etlJobScheduleItem.OccursEveryMinuteEndingAt),
				new SqlParameter("etl_job_name", etlJobScheduleItem.JobName),
				new SqlParameter("etl_relative_period_start", etlJobScheduleItem.RelativePeriodStart),
				new SqlParameter("etl_relative_period_end", etlJobScheduleItem.RelativePeriodEnd),
				new SqlParameter("etl_datasource_id", etlJobScheduleItem.DataSourceId),
				new SqlParameter("description", etlJobScheduleItem.Description),
				new SqlParameter("tenant_name", etlJobScheduleItem.TenantName)
			};

			return
				(int)
				HelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.etl_job_save_schedule", parameterList,
					dataMartConnectionString);
		}

		public DataTable GetSchedulePeriods(int scheduleId)
		{
			var parameterList = new[] { new SqlParameter("schedule_id", scheduleId) };
			DataSet ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.etl_job_get_schedule_periods",
				parameterList,
				dataMartConnectionString);
			return ds.Tables[0];
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

		public void DisableScheduleJob(int scheduleId)
		{
			var parameterList = new[] { new SqlParameter("schedule_id", scheduleId) };

			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_job_disable_schedule", parameterList,
				dataMartConnectionString);
		}
	}
}