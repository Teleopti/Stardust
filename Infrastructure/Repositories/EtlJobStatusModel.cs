using System;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class EtlJobStatusModel
	{
		public string job_name { get; set; }
		public string business_unit_name { get; set; }
		public DateTime job_start_time { get; set; }
		public DateTime job_end_time { get; set; }
		public int job_duration_s { get; set; }
		public int job_affected_rows { get; set; }
		public string schedule_name { get; set; }
		public string jobstep_name { get; set; }
		public int jobstep_duration_s { get; set; }
		public int jobstep_affected_rows { get; set; }
		public string exception_msg { get; set; }
		public string exception_trace { get; set; }
		public string inner_exception_msg { get; set; }
		public string inner_exception_trace { get; set; }
	}
}