using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.ScheduleStaffingPossibility
{
	public class PeriodStaffingPossibilityViewModel
	{
		public string Date { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime EndTime { get; set; }

		public int Possibility { get; set; }
	}
}