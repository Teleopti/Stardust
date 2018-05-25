using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule
{
	public class TimeLineViewModel
	{
	}

	public class TeamScheduleTimeLineViewModelToggle75989Off : TimeLineViewModel
	{
		public string HourText { get; set; }
		public int LengthInMinutesToDisplay { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}

	public class TeamScheduleTimeLineViewModel : TimeLineViewModel
	{
		public TimeSpan Time { get; set; }
		public string TimeLineDisplay { get; set; }
		public decimal PositionPercentage { get; set; }
		public string TimeFixedFormat { get; set; }
	}

}