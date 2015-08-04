using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class SeatBookingReportViewModel
	{
		public List<SeatBookingByDateViewModel> SeatBookingsByDate { get; set; }
		public int TotalRecordCount { get; set; }

		public SeatBookingReportViewModel(IEnumerable<SeatBookingByDateViewModel> seatBookingsByDate, int recordCount)
		{
			SeatBookingsByDate = new List<SeatBookingByDateViewModel>(seatBookingsByDate);
			TotalRecordCount = recordCount;
		}
	}
}