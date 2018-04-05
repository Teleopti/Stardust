using System;
using System.Collections.Generic;
using Teleopti.Wfm.Administration.Controllers;

namespace Teleopti.Wfm.Administration.Models
{
	public class EtlScheduleJobModel
	{
		public int ScheduleId { get; set; }
		public string ScheduleName { get; set; }
		public string Description { get; set; }
		public string JobName { get; set; }
		public bool Enabled { get; set; }
		public string Tenant { get; set; }
		public DateTime DailyFrequencyStart { get; set; }
		public DateTime DailyFrequencyEnd { get; set; }
		public string DailyFrequencyMinute { get; set; }
		public JobPeriodRelative[] RelativePeriods { get; set; }
		public int LogDataSourceId { get; set; }
	}
}