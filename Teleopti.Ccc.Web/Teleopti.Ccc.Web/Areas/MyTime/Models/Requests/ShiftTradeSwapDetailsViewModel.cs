using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeSwapDetailsViewModel
	{
		public ShiftTradePersonScheduleViewModel From { get; set; }

		public ShiftTradePersonScheduleViewModel To { get; set; }

		public DateOnly DateFrom { get; set; }

		public DateOnly DateTo { get; set; }
	}
}