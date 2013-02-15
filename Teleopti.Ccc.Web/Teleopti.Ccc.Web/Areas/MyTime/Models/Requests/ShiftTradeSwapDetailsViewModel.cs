using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeSwapDetailsViewModel
	{
		public ShiftTradePersonScheduleViewModel From { get; set; }

		public ShiftTradePersonScheduleViewModel To { get; set; }

		//public IEnumerable<ShiftTradeTimeLineHoursViewModel> TimeLineHours { get; set; }

		//public int TimeLineLengthInMinutes { get; set; }

		public DateTime DateFrom { get; set; }

		public DateTime DateTo { get; set; }
	}
}