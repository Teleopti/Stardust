using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule
{
	public class TeamScheduleLayerViewModel
	{
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public int LengthInMinutes { get; set; }
		public string Color { get; set; }
		public string TitleHeader { get; set; }
		public bool IsAbsenceConfidential { get; set; }
		public bool IsOvertime { get; set; }
		public string TitleTime { get; set; }
	}
}