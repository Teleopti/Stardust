using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeScheduleLayerViewModel
	{
		public string Payload { get; set; } //Prob remove
		public int ElapsedMinutesSinceShiftStart { get; set; }	//Prob remove

		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public int LengthInMinutes { get; set; }
		public string Color { get; set; }
		public string TitleHeader { get; set; }
		public bool IsAbsenceConfidential { get; set; }
		public string TitleTime { get; set; }
	}
}