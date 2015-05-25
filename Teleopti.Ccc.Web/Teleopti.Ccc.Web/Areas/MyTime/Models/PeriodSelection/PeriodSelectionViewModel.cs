using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.PeriodSelection
{
	public class PeriodSelectionViewModel
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Display { get; set; }
		public string Date { get; set; }
		public PeriodDateRangeViewModel SelectedDateRange { get; set; }
		public PeriodDateRangeViewModel SelectableDateRange { get; set; }
		public PeriodNavigationViewModel PeriodNavigation { get; set; }
	}
}