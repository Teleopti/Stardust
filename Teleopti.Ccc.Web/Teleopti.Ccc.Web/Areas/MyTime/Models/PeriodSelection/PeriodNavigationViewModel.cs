namespace Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection
{
	public class PeriodNavigationViewModel
	{
		public string FirstDateNextPeriod { get; set;}
		public bool HasNextPeriod { get; set; }
		public string LastDatePreviousPeriod { get; set; }
		public bool HasPrevPeriod { get; set; }
		public bool CanPickPeriod { get; set; }
	}
}