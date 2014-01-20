using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeRequestsPeriodViewModel
	{
		public bool HasWorkflowControlSet { get; set; }

		public int OpenPeriodRelativeStart { get; set; }

		public int OpenPeriodRelativeEnd { get; set; }

		public int NowYear { get; set; }

		public int NowMonth { get; set; }

		public int NowDay { get; set; }
	}
}