﻿namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class GroupScheduleDayOffViewModel
	{
		public string DayOffName { get; set; }
		public string Start { get; set; }
		public string End { get; set; }
		public string StartInUtc { get; set; }
		public string EndInUtc { get; set; }
		public int Minutes { get; set; }
	}
}