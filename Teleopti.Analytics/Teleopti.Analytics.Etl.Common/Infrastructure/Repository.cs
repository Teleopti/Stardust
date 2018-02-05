using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class Repository : IJobLogRepository, IRunControllerRepository, IJobHistoryRepository
	{
		private readonly string _connectionString;

		public Repository(string connectionString)
		{
			_connectionString = connectionString;
		}

		

		public DataTable GetLog()
		{
			DataSet ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.etl_log_get_today", null,
														_connectionString);
			return ds.Tables[0];
		}

		public int SaveLogPre(int scheduleId)
		{
			var parameterList = new[]
			{
				new SqlParameter("schedule_id", scheduleId)
			};
			var id = (int)HelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.etl_log_save_init", parameterList, _connectionString);
			return id;
		}

		public void SaveLogPost(IEtlJobLog etlJobLogItem, IJobResult jobResult)
		{
			var parameterList = new[]
									{
										new SqlParameter("job_execution_id", etlJobLogItem.ScopeIdentity),
										new SqlParameter("job_name", jobResult.Name),
										new SqlParameter("schedule_id", etlJobLogItem.ScheduleId), 
										jobResult.CurrentBusinessUnit == null ? new SqlParameter("business_unit_code", DBNull.Value) : new SqlParameter("business_unit_code", jobResult.CurrentBusinessUnit.Id),
										jobResult.CurrentBusinessUnit == null ? new SqlParameter("business_unit_name", DBNull.Value) : new SqlParameter("business_unit_name", jobResult.CurrentBusinessUnit.Description.Name),
										new SqlParameter("start_datetime", etlJobLogItem.StartTime),
										new SqlParameter("end_datetime", etlJobLogItem.EndTime),
										new SqlParameter("duration", Convert.ToInt32(jobResult.Duration)),
										new SqlParameter("affected_rows", jobResult.RowsAffected),
										new SqlParameter("error_msg", DBNull.Value)
									};

			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_log_save_post", parameterList,
											_connectionString);
		}

		public void SaveLogStepPost(IEtlJobLog etlJobLogItem, IJobStepResult jobStepResult)
		{
			Exception jobStepException = jobStepResult.JobStepException;
			Exception innerException = null;
			if (jobStepException != null)
			{
				innerException = jobStepResult.JobStepException.InnerException;
			}

			var parameterList = new[]
									{
										new SqlParameter("job_execution_id", etlJobLogItem.ScopeIdentity),
										jobStepResult.CurrentBusinessUnit == null ? new SqlParameter("business_unit_code", DBNull.Value) : new SqlParameter("business_unit_code", jobStepResult.CurrentBusinessUnit.Id),
										jobStepResult.CurrentBusinessUnit == null ? new SqlParameter("business_unit_name", DBNull.Value) : new SqlParameter("business_unit_name", jobStepResult.CurrentBusinessUnit.Description.Name),
										new SqlParameter("jobstep_name", jobStepResult.Name),
										jobStepResult.Duration.HasValue ? new SqlParameter("duration", Convert.ToInt32(jobStepResult.Duration.Value)): new SqlParameter("duration", DBNull.Value),
										jobStepResult.RowsAffected.HasValue ? new SqlParameter("affected_rows", Convert.ToInt32(jobStepResult.RowsAffected.Value)): new SqlParameter("affected_rows", DBNull.Value),
										jobStepException == null ? new SqlParameter("exception_message", DBNull.Value) : new SqlParameter("exception_message",jobStepException.Message),                                        
										jobStepException == null ? new SqlParameter("exception_stacktrace", DBNull.Value) : new SqlParameter("exception_stacktrace",jobStepException.StackTrace),
										innerException == null ? new SqlParameter("inner_exception_message", DBNull.Value) : new SqlParameter("inner_exception_message",innerException.Message),                                        
										innerException == null ? new SqlParameter("inner_exception_stacktrace", DBNull.Value) : new SqlParameter("inner_exception_stacktrace",innerException.StackTrace),
									};

			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_log_jobstep_save", parameterList,
											_connectionString);

		}


		public DataTable GetEtlJobHistory(DateTime startDate, DateTime endDate, Guid businessUnitId, bool showOnlyErrors)
		{
			var parameterList = new[]
									{
										new SqlParameter("start_date", startDate),
										new SqlParameter("end_date", endDate),
										new SqlParameter("business_unit_id", businessUnitId),
										new SqlParameter("show_only_errors", showOnlyErrors)
									};


			var ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.etl_job_execution_history", parameterList, _connectionString);
			return ds.Tables[0];
		}




		public bool IsAnotherEtlRunningAJob(out IEtlRunningInformation etlRunningInformation)
		{
			etlRunningInformation = null;
			var ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.sys_etl_job_running_info_get", null,
				_connectionString);

			if (ds != null && ds.Tables.Count > 0)
			{
				if (ds.Tables[0].Rows.Count >= 1)  //Shouldn't be greater than 1 but until we know what's going on with #47243..
				{
					var row = ds.Tables[0].Rows[0];
					etlRunningInformation = new EtlRunningInformation
					{
						ComputerName = (string) row["computer_name"],
						StartTime = (DateTime) row["start_time"],
						JobName = (string) row["job_name"],
						IsStartedByService = (bool) row["is_started_by_service"]
					};
					return true;
				}
			}
			return false;
		}

		public DataTable BusinessUnitsIncludingAllItem
		{
			get
			{
				var ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.sys_business_unit_all_get", null,
														_connectionString);
				return ds.Tables[0];
			}
		}
	}
}