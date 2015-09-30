using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISeatBookingRepository : IRepository<ISeatBooking>, ILoadAggregateFromBroker<ISeatBooking>
	{
		IList<ISeatBooking> LoadSeatBookingsForDateOnlyPeriod(DateOnlyPeriod period);
		IList<ISeatBooking> LoadSeatBookingsForDateOnlyPeriod (IPerson person, DateOnlyPeriod period);
		ISeatBooking LoadSeatBookingForPerson (DateOnly date, IPerson person);
		IList<ISeatBooking> LoadSeatBookingsForDay (DateOnly date);
		void RemoveSeatBookingsForSeats(IEnumerable<ISeat> seats);
		void RemoveSeatBookingsForSeat (ISeat seat);
		IList<ISeatBooking> GetSeatBookingsForSeat (ISeat seat);
		ISeatBookingReportModel LoadSeatBookingsReport (ISeatBookingReportCriteria criteria, Paging paging);
		IList<ISeatBooking> LoadSeatBookingsIntersectingDay (DateOnly dateOnly, Guid locationId);
		IList<ISeatBooking> LoadSeatBookingsForSeatIntersectingDay (DateOnly date, Guid seatId);
		IList<ISeatBooking> LoadSeatBookingsForSeatsIntersectingDay (DateOnly dateOnly, IList<Guid> seatIds);
	}
}