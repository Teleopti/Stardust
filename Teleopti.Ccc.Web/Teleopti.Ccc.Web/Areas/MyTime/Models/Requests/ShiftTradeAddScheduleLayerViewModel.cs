using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeAddScheduleLayerViewModel
	{
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public int LengthInMinutes { get; set; }
		public string Color { get; set; }
		public string TitleHeader { get; set; }
		public bool IsAbsenceConfidential { get; set; }
		public string TitleTime { get; set; }
	}
}