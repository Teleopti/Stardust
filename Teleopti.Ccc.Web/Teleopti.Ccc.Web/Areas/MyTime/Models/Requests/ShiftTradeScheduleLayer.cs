namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Requests
{
	public class ShiftTradeScheduleLayer
	{
		public string Payload { get; set; }
		public double LengthInMinutes { get; set; }
		public string Color { get; set; }
		public string StartTimeText { get; set; }
		public string EndTimeText { get; set; }
	}
}