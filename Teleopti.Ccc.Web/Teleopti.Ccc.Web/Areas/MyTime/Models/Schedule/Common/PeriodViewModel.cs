using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common
{
	public class PeriodViewModel
	{
		public string Title { get; set; }
		public string TimeSpan { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Summary { get; set; }
		public string StyleClassName { get; set; }
		public MeetingViewModel Meeting { get; set; }
		public decimal StartPositionPercentage { get; set; }
		public decimal EndPositionPercentage { get; set; }
		public string Color { get; set; }
		public bool IsOvertime { get; set; }
	}
}