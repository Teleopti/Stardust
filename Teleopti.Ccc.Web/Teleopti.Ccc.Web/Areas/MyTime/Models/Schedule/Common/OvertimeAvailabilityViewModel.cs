namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common
{
	public class OvertimeAvailabilityViewModel
	{
		public bool HasOvertimeAvailability { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public bool EndTimeNextDay { get; set; }
		public string DefaultStartTime { get; set; }
		public string DefaultEndTime { get; set; }
		public bool DefaultEndTimeNextDay { get; set; }
	}
}