using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability
{
	public class StudentAvailabilityDayViewModel
	{
		public string Date { get; set; }
		public string AvailableTimeSpan { get; set; }
		public BankHolidayCalendarViewModel BankHolidayCalendar { get; set; }
	}
}