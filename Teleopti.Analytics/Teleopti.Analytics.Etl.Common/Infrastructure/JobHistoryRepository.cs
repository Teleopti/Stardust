using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.Util;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.JobHistory;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class JobHistoryRepository : IJobHistoryRepository
	{
		public IList<JobHistoryViewModel> GetEtlJobHistory(DateTime startDate, DateTime endDate, List<Guid> businessUnitIds, bool showOnlyErrors, string connectionString)
		{
			var parameterList = new[]
									{
										new SqlParameter("start_date", startDate),
										new SqlParameter("end_date", endDate),
										new SqlParameter("business_unit_id", string.Join(",", businessUnitIds)),
										new SqlParameter("show_only_errors", showOnlyErrors)
									};


			var ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.etl_job_execution_history", parameterList, connectionString);

			if (!ds.Tables.Any() || ds.Tables[0] == null)
				return null;

			int previousJobExecutionId = -99;
			JobHistoryViewModel jobModel = null;

			var returnList = new List<JobHistoryViewModel>();
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				if ((int)row["job_execution_id"] != previousJobExecutionId)
				{
					if (jobModel != null)
						returnList.Add(jobModel);

					jobModel = new JobHistoryViewModel
					{
						Name = (string)row["job_name"],
						BusinessUnitName = (string)row["business_unit_name"],
						StartTime = (DateTime)row["job_start_time"],
						EndTime = (DateTime)row["job_end_time"],
						Duration = new TimeSpan(0, 0, (int)row["job_duration_s"]),
						RowsAffected = (int)row["job_affected_rows"],
						ScheduleName =
										   (int)row["schedule_id"] == -1
											   ? "Manual Etl"
											   : row["schedule_name"] == DBNull.Value
												? string.Empty
												: (string)row["schedule_name"],
						Success = true
					};
				}

				var jobStepModel = new JobStepHistoryViewModel
				{
					Name = (string)row["jobstep_name"],
					Duration = new TimeSpan(0, 0, (int)row["jobstep_duration_s"]),
					RowsAffected = row["jobstep_affected_rows"] == DBNull.Value ? 0 : (int)row["jobstep_affected_rows"],
					Success = row["exception_msg"] == DBNull.Value,
					ErrorMessage = row["exception_msg"] == DBNull.Value ? string.Empty : (string)row["exception_msg"],
					ErrorStackTrace = row["exception_trace"] == DBNull.Value ? string.Empty : (string)row["exception_trace"],
					InnerErrorMessage = row["inner_exception_msg"] == DBNull.Value ? string.Empty : (string)row["inner_exception_msg"],
					InnerErrorStackTrace = row["inner_exception_trace"] == DBNull.Value ? string.Empty : (string)row["inner_exception_trace"]
				};
				if (!string.IsNullOrEmpty(jobStepModel.ErrorMessage))
				{
					jobModel.ErrorMessage = jobStepModel.ErrorMessage;
					jobModel.ErrorStackTrace = jobStepModel.ErrorStackTrace;
					jobModel.InnerErrorMessage = jobStepModel.InnerErrorMessage;
					jobModel.InnerErrorStackTrace = jobStepModel.InnerErrorStackTrace;

					jobModel.Success = false;
				}
				jobModel.AddJobStepHistory(jobStepModel);

				previousJobExecutionId = (int)row["job_execution_id"];
			}

			if (jobModel != null)
				returnList.Add(jobModel);

			return returnList;
		}

		public IList<BusinessUnitItem> GetBusinessUnitsIncludingAll(string connectionString)
		{
			var ds = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.sys_business_unit_all_get", null,
				connectionString);
			if (!ds.Tables.Any() || ds.Tables[0] == null)
			{
				return null;
			}
			return
			(from DataRow row in ds.Tables[0].Rows
				select new BusinessUnitItem
				{
					Id = (Guid) row["id"],
					Name = (string) row["name"]
				}).ToList();
		}
	}
}