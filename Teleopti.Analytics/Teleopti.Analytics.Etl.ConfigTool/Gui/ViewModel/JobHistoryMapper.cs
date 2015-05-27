using System;
using System.Configuration;
using System.Data;
using Teleopti.Analytics.Etl.TransformerInfrastructure;

namespace Teleopti.Analytics.Etl.ConfigTool.Gui.ViewModel
{
	public static class JobHistoryMapper
	{
		public static JobHistoryTreeViewModel Map(DateTime startDate, DateTime endDate, Guid businessUnitId, bool showOnlyErrors)
		{
			var repository = new Repository(ConfigurationManager.AppSettings["datamartConnectionString"]);

			DataTable table = repository.GetEtlJobHistory(startDate, endDate, businessUnitId, showOnlyErrors);

			if (table == null)
				return null;

			int previousJobExecutionId = -99;
			JobHistoryViewModel jobModel = null;
			var returnList = new JobHistoryTreeViewModel();

			foreach (DataRow row in table.Rows)
			{
				if ((int)row["job_execution_id"] != previousJobExecutionId)
				{
					if (jobModel != null)
						returnList.Add(jobModel);

					jobModel = new JobHistoryViewModel
					           	{
					           		Name = (string) row["job_name"],
					           		BusinessUnitName = (string) row["business_unit_name"],
					           		StartTime = (DateTime) row["job_start_time"],
					           		EndTime = (DateTime) row["job_end_time"],
					           		Duration = new TimeSpan(0, 0, (int) row["job_duration_s"]),
					           		RowsAffected = (int) row["job_affected_rows"],
					           		ScheduleName =
					           			(int) row["schedule_id"] == -1
					           				? "Manual Etl"
					           				: row["schedule_name"] == DBNull.Value 
												? string.Empty 
												: (string) row["schedule_name"],
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
	}
}
