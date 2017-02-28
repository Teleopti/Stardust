using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface ISeatBookingReportModel
	{
		IEnumerable<IPersonScheduleWithSeatBooking> SeatBookings { get; set; }
		int RecordCount { get; set; }
	}
}