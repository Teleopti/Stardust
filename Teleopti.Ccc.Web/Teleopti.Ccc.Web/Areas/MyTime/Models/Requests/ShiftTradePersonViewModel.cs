using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradePersonViewModel
	{
		public string Name { get; set; }
		public IEnumerable<ShiftTradeScheduleLayerViewModel> ScheduleLayers { get; set; }
	}
}