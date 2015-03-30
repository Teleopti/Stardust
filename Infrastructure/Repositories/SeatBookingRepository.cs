using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class SeatBookingRepository : Repository<ISeatBooking>, ISeatBookingRepository
	{
		public SeatBookingRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{
		}

		public SeatBookingRepository(IUnitOfWorkFactory unitOfWorkFactory)
			: base(unitOfWorkFactory)
		{
		}

		public SeatBookingRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork)
		{
		}

		public ISeatBooking LoadAggregate(Guid id)
		{
			return Session.Query<ISeatBooking>()
				.FirstOrDefault(booking => booking.Id == id);
		}

		public ISeatBooking LoadSeatBookingForPerson(DateOnly date, IPerson person)
		{
			return Session.Query<ISeatBooking>().SingleOrDefault(booking => (booking.StartDateTime.Date == date.Date && booking.Person == person));
		}

		public IList<ISeatBooking> LoadSeatBookingsForDateOnlyPeriod(DateOnlyPeriod period)
		{
			return Session.Query<ISeatBooking>()
				.Where(booking => booking.StartDateTime.Date >= period.StartDate.Date
					&& booking.EndDateTime.Date <= period.EndDate.Date).ToList();
		}

		public IList<ISeatBooking> LoadSeatBookingsForDay(DateOnly date)
		{
			return Session.Query<ISeatBooking>()
				.Where(booking => booking.StartDateTime.Date == date.Date).ToList();
		}

		public void RemoveSeatBookingsForSeats(IEnumerable<ISeat>seats)
		{
			seats.ForEach (RemoveSeatBookingsForSeat);
		}

		public void RemoveSeatBookingsForSeat(ISeat seat)
		{
			Session.Query<ISeatBooking>().Where(booking => booking.Seat == seat)
				.ForEach(booking => Session.Delete(booking));
		}

	}
}
