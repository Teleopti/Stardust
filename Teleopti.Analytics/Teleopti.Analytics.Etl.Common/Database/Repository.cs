using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;


namespace Teleopti.Analytics.Etl.Common.Database
{
    public class Repository : IScheduleRepository, ILogRepository
    {
        private readonly string _connectionString;

        public Repository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable GetSchedulePeriods(int scheduleId)
        {
            var parameterList = new List<SqlParameter>
                                    {
                                        new SqlParameter("schedule_id", scheduleId), 
                                    };
            DataSet ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.etl_job_get_schedule_periods",
                                                        parameterList,
                                                        _connectionString);
            return ds.Tables[0];
        }

        public DataTable GetSchedules()
        {
            DataSet ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.etl_job_get_schedules", null,
                                                        _connectionString);
            return ds.Tables[0];
        }

        public DataTable GetLog()
        {
            DataSet ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.etl_log_get_today", null,
                                                        _connectionString);
            return ds.Tables[0];
        }

        public int SaveLogPre()
        {
            var id = (int)HelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.etl_log_save_init", _connectionString);
            return id;
        }

        public void SaveLogPost(IEtlLog etlLogItem, IJobResult jobResult)
        {
            var parameterList = new List<SqlParameter>
                                    {
                                        new SqlParameter("job_execution_id", etlLogItem.ScopeIdentity),
                                        new SqlParameter("job_name", jobResult.Name),
                                        new SqlParameter("schedule_id", etlLogItem.ScheduleId), 
                                        jobResult.CurrentBusinessUnit == null ? new SqlParameter("business_unit_code", DBNull.Value) : new SqlParameter("business_unit_code", jobResult.CurrentBusinessUnit.Id),
                                        jobResult.CurrentBusinessUnit == null ? new SqlParameter("business_unit_name", DBNull.Value) : new SqlParameter("business_unit_name", jobResult.CurrentBusinessUnit.Description.Name),
                                        new SqlParameter("start_datetime", etlLogItem.StartTime),
                                        new SqlParameter("end_datetime", etlLogItem.EndTime),
                                        new SqlParameter("duration", Convert.ToInt32(jobResult.Duration)),
                                        new SqlParameter("affected_rows", jobResult.RowsAffected),
                                        new SqlParameter("error_msg", DBNull.Value)
                                    };

            HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_log_save_post", parameterList,
                                            _connectionString);
        }

        public void SaveLogStepPost(IEtlLog etlLogItem, IJobStepResult jobStepResult)
        {
            Exception jobStepException = jobStepResult.JobStepException;
            Exception innerException = null;
            if (jobStepException != null)
            {
                innerException = jobStepResult.JobStepException.InnerException;
            }

            var parameterList = new List<SqlParameter>
                                    {
                                        new SqlParameter("job_execution_id", etlLogItem.ScopeIdentity),
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


        public void AddJobStep(IEtlLog etlLogItem, IJobStepResult jobStepResult)
        {

            var parameterList = new List<SqlParameter>
                                    {
                                        new SqlParameter("job_execution_id", etlLogItem.ScopeIdentity),
                                        new SqlParameter("jobstep_name", jobStepResult.Name)
                                    };


            HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_log_jobstep_save", parameterList,
                                            _connectionString);
        }

        public int SaveSchedule(IEtlSchedule etlScheduleItem)
        {
            var parameterList = new List<SqlParameter>
                                    {
                                        new SqlParameter("schedule_id", etlScheduleItem.ScheduleId),
                                        new SqlParameter("schedule_name", etlScheduleItem.ScheduleName),
                                        new SqlParameter("enabled", etlScheduleItem.Enabled),
                                        new SqlParameter("schedule_type", etlScheduleItem.ScheduleType),
                                        new SqlParameter("occurs_daily_at", etlScheduleItem.OccursOnceAt),
                                        new SqlParameter("occurs_every_minute", etlScheduleItem.OccursEveryMinute),
                                        new SqlParameter("recurring_starttime", etlScheduleItem.OccursEveryMinuteStartingAt),
                                        new SqlParameter("recurring_endtime", etlScheduleItem.OccursEveryMinuteEndingAt),
                                        new SqlParameter("etl_job_name", etlScheduleItem.JobName),
                                        new SqlParameter("etl_relative_period_start", etlScheduleItem.RelativePeriodStart),
                                        new SqlParameter("etl_relative_period_end", etlScheduleItem.RelativePeriodEnd),
                                        new SqlParameter("etl_datasource_id", etlScheduleItem.DataSourceId),
                                        new SqlParameter("description", etlScheduleItem.Description)
                                    };

            return
                (int)
                HelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.etl_job_save_schedule", parameterList,
                                              _connectionString);
        }

        public void SaveSchedulePeriods(IEtlSchedule etlScheduleItem)
        {
            foreach (IEtlJobRelativePeriod relativePeriod in etlScheduleItem.RelativePeriodCollection)
            {
                var parameterList = new List<SqlParameter>
                                    {
                                        new SqlParameter("schedule_id", etlScheduleItem.ScheduleId),
                                        new SqlParameter("job_name", relativePeriod.JobCategoryName),
                                        new SqlParameter("relative_period_start", relativePeriod.RelativePeriod.Minimum),
                                        new SqlParameter("relative_period_end", relativePeriod.RelativePeriod.Maximum),
                                    };


                HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_job_save_schedule_period", parameterList,
                                                _connectionString);
            }
        }

        public void DeleteSchedule(int scheduleId)
        {
            var parameterList = new List<SqlParameter> { new SqlParameter("schedule_id", scheduleId) };

            HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_job_delete_schedule", parameterList,
                                            _connectionString);
        }
    }
}