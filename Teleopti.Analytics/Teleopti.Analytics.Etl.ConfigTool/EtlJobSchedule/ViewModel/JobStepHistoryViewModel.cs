using System;

namespace Teleopti.Analytics.Etl.ConfigTool.EtlJobSchedule.ViewModel
{
	public class JobStepHistoryViewModel : IJobHistory
	{
		public string Name { get; set; }
		public TimeSpan Duration { get; set; }
		public int RowsAffected { get; set; }
		public bool Success { get; set; }
		public string ErrorMessage { get; set; }
		public string ErrorStackTrace { get; set; }
		public string InnerErrorMessage { get; set; }
		public string InnerErrorStackTrace { get; set; }
	}
}