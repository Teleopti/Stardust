using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeRequestsPreparationViewModel
	{
		public bool HasWorkflowControlSet { get; set; }

		public IEnumerable<ShiftTradeScheduleLayer> MySchedulelayers { get; set; }
	}
}