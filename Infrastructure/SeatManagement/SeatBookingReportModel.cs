using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.SeatManagement
{
	public class SeatBookingReportModel : ISeatBookingReportModel
	{
		public IEnumerable<IPersonScheduleWithSeatBooking> SeatBookings { get; set; }
		public int RecordCount { get; set; }
	}
}