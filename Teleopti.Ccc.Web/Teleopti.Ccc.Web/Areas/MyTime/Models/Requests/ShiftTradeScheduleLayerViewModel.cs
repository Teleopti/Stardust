namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeScheduleLayerViewModel
	{
		public string Payload { get; set; }
		public int LengthInMinutes { get; set; }
		public string Color { get; set; }
		public int ElapsedMinutesSinceShiftStart { get; set; }
		public string Title { get; set; }
	}
}