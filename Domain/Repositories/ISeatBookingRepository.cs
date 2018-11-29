using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISeatBookingRepository : IRepository<ISeatBooking>
	{
		IList<ISeatBooking> LoadSeatBookingsForDateOnlyPeriod(DateOnlyPeriod period);
		ISeatBooking LoadSeatBookingForPerson (DateOnly date, IPerson person);
		IList<ISeatBooking> LoadSeatBookingsForDay (DateOnly date);
		void RemoveSeatBookingsForSeats(IEnumerable<ISeat> seats);
		void RemoveSeatBookingsForSeat (ISeat seat);
		IList<ISeatBooking> GetSeatBookingsForSeat (ISeat seat);
		ISeatBookingReportModel LoadSeatBookingsReport (ISeatBookingReportCriteria criteria, Paging paging);
		IList<ISeatBooking> LoadSeatBookingsIntersectingDateTimePeriod(DateTimePeriod dateTimePeriod, Guid locationId);
		IList<ISeatBooking> LoadSeatBookingsIntersectingDateTimePeriod(DateTimePeriod dateTImePeriod, IList<Guid> seatIds);
	}
}