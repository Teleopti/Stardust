namespace Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.Common
{
	public class OvertimeAvailabilityPeriodViewModel : PeriodViewModel
	{
		public bool IsOvertimeAvailability { get; set; }
		public OvertimeAvailabilityViewModel OvertimeAvailabilityYesterday { get; set; }
	}
}