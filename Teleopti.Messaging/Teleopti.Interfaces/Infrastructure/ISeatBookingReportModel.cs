using System.Collections.Generic;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface ISeatBookingReportModel
	{
		IEnumerable<IPersonScheduleWithSeatBooking> SeatBookings { get; set; }
		int RecordCount { get; set; }
	}
}