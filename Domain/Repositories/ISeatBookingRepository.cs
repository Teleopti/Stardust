using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ISeatBookingRepository : IRepository<ISeatBooking>, ILoadAggregateFromBroker<ISeatBooking>
	{
		IList<ISeatBooking> LoadSeatBookingsForDateOnlyPeriod(DateOnlyPeriod period);
		ISeatBooking LoadSeatBookingForPerson (DateOnly date, IPerson person);
		IList<ISeatBooking> LoadSeatBookingsForDay (DateOnly date);
		void RemoveSeatBookingsForSeats(IEnumerable<ISeat> seats);
		void RemoveSeatBookingsForSeat (ISeat seat);
		
	}
}