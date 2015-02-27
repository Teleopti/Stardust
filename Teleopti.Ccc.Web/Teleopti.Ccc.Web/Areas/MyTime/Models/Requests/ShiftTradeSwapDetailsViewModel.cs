using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeSwapDetailsViewModel
	{
		public string PersonFrom { get; set; }

		public string PersonTo { get; set; }

		public ShiftTradeEditPersonScheduleViewModel From { get; set; }

		public ShiftTradeEditPersonScheduleViewModel To { get; set; }

		public IEnumerable<TimeLineViewModelReworked> TimeLineHours { get; set; }

		public DateTime TimeLineStartDateTime { get; set; }
		public DateTime Date { get; set; }
	}
}