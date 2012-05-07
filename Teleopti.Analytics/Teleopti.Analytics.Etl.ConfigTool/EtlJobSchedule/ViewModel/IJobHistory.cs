using System;

namespace Teleopti.Analytics.Etl.ConfigTool.EtlJobSchedule.ViewModel
{
	public interface IJobHistory
	{
		string Name { get; set; }
		TimeSpan Duration { get; set; }
		int RowsAffected { get; set; }
		bool Success { get; set; }
		string ErrorMessage { get; set; }
		string ErrorStackTrace { get; set; }
		string InnerErrorMessage { get; set; }
		string InnerErrorStackTrace { get; set; }
	}
}