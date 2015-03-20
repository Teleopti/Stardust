

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
			return Session.Query<ISeatBooking>().SingleOrDefault(booking => (booking.StartDateTime.Date == date && booking.Person == person));
		}

		public IList<ISeatBooking> LoadSeatBookingsForDateOnlyPeriod(DateOnly start, DateOnly end)
		{
			return Session.Query<ISeatBooking>()
				.Where(booking => (booking.StartDateTime.Date >= start
					&& booking.EndDateTime.Date <= end)).ToList();
		}

		public IList<ISeatBooking> LoadSeatBookingsForPeriod(DateTime start, DateTime end)
		{
			return Session.Query<ISeatBooking>()
				.Where(booking => (booking.StartDateTime >= start
					&& booking.EndDateTime <= end)).ToList();
		}

		public IList<ISeatBooking> LoadSeatBookingsForDay(DateOnly date)
		{
			return Session.Query<ISeatBooking>()
				.Where(booking => (booking.StartDateTime.Date == date)).ToList();
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
