using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.Common.JobHistory
{
	public class JobHistoryViewModel : IJobHistory
	{
		public string Name { get; set; }
		public string BusinessUnitName { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public TimeSpan Duration { get; set; }
		public int RowsAffected { get; set; }
		public string ScheduleName { get; set; }
		public IList<JobStepHistoryViewModel> JobStepsHistory { get; private set; }
		public bool Success { get; set; }
		public string ErrorMessage { get; set; }
		public string ErrorStackTrace { get; set; }
		public string InnerErrorMessage { get; set; }
		public string InnerErrorStackTrace { get; set; }
		
		public void AddJobStepHistory(JobStepHistoryViewModel jobStepModel)
		{
			if (JobStepsHistory == null)
				JobStepsHistory = new List<JobStepHistoryViewModel>();

			JobStepsHistory.Add(jobStepModel);
		}
	}
}
