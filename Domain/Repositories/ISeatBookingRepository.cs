using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISeatBookingRepository : IRepository<ISeatBooking>, IWriteSideRepository<ISeatBooking>, ILoadAggregateFromBroker<ISeatBooking>
	{
		IList<ISeatBooking> LoadSeatBookingsForPeriod(DateTime start, DateTime end);
		ISeatBooking LoadSeatBookingForPerson (DateOnly date, IPerson person);
		IList<ISeatBooking> LoadSeatBookingsForDay (DateOnly date);
		void RemoveSeatBookingsForSeats(IEnumerable<ISeat> seats);
		void RemoveSeatBookingsForSeat (ISeat seat);
		
	}
}