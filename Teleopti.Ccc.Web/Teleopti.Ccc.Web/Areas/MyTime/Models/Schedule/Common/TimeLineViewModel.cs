using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common
{
	public class TimeLineViewModel
	{
		public TimeSpan Time { get; set; }
		public string TimeLineDisplay { get; set; }
		public decimal PositionPercentage { get; set; }
		public string TimeFixedFormat { get; set; }
	}
}