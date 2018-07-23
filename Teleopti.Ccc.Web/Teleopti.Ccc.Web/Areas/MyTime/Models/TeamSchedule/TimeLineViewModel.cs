using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule
{
	public class TeamScheduleTimeLineViewModelToggle75989Off
	{
		public string HourText { get; set; }
		public int LengthInMinutesToDisplay { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}

	public class TeamScheduleTimeLineViewModel
	{
		public DateTime Time { get; set; }
		public string TimeLineDisplay { get; set; }
		public decimal PositionPercentage { get; set; }
		public string TimeFixedFormat { get; set; }
	}

}