using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSeatBookingRepository : ISeatBookingRepository, IEnumerable<ISeatBooking>
	{

		private readonly IList<ISeatBooking> _seatBookings = new List<ISeatBooking>();


		public FakeSeatBookingRepository()
		{
		}

		public void Has(ISeatBooking booking)
		{
			_seatBookings.Add(booking);
		}

		
		public void Add (ISeatBooking entity)
		{
			_seatBookings.Add (entity);
		}

		public void Remove (ISeatBooking entity)
		{
			_seatBookings.Remove (entity);
		}

		public ISeatBooking Get (Guid id)
		{
			return _seatBookings.SingleOrDefault(booking => booking.Id == id);
		}

		public IList<ISeatBooking> LoadAll()
		{
			throw new NotImplementedException();
		}

		public ISeatBooking Load (Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange (IEnumerable<ISeatBooking> entityCollection)
		{
			entityCollection.ForEach (Add);
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		//ISeatBooking ILoadAggregateByTypedId<ISeatBooking, Guid>.LoadAggregate (Guid id)
		//{
		//	return LoadAggregate (id);
		//}

		public ISeatBooking LoadAggregate (Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<ISeatBooking> LoadSeatBookingsForPeriod (DateTime start, DateTime end)
		{
			return _seatBookings
				.Where(booking => (booking.StartDateTime >= start
					&& booking.EndDateTime <= end)).ToList();
		}

		public ISeatBooking LoadSeatBookingForPerson (DateOnly date, IPerson person)
		{
			return _seatBookings.SingleOrDefault(booking => (booking.StartDateTime.Date == date && booking.Person == person));
		}

		public IList<ISeatBooking> LoadSeatBookingsForDay (DateOnly date)
		{
			return _seatBookings
				.Where(booking => (booking.StartDateTime.Date == date)).ToList();
		}

		public void RemoveSeatBookingsForSeats (IEnumerable<ISeat> seats)
		{
			seats.ForEach(RemoveSeatBookingsForSeat);
		}

		public void RemoveSeatBookingsForSeat (ISeat seat)
		{
			_seatBookings.Where(booking => booking.Seat == seat)
				.ForEach(booking => _seatBookings.Remove(booking));
		}

		public IEnumerator<ISeatBooking> GetEnumerator()
		{
			return _seatBookings.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}