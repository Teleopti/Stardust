namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeScheduleLayerViewModel
	{
		public string Payload { get; set; }
		public int LengthInMinutes { get; set; }
		public string Color { get; set; }
		public string StartTimeText { get; set; }
		public string EndTimeText { get; set; }
		public int ElapsedMinutesSinceShiftStart { get; set; }
		public bool IsDayOff { get; set; }
	}
}