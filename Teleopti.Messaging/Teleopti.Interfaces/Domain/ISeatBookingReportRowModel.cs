using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatBookingReportModel
	{
		IEnumerable<ISeatBooking> SeatBookings { get; set; }
		int RecordCount { get; set; }
	}
	
}