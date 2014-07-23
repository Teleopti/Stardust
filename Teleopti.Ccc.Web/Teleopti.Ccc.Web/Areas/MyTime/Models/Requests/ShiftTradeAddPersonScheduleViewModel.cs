using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeAddPersonScheduleViewModel
	{
		public IEnumerable<ShiftTradeAddScheduleLayerViewModel> ScheduleLayers { get; set; }
		public string Name { get; set; }
		public DateTime StartTimeUtc { get; set; }
		public Guid PersonId { get; set; }
		public DateTime? MinStart { get; set; }
		public bool IsDayOff { get; set; }
		public int Total { get; set; }
	}
}