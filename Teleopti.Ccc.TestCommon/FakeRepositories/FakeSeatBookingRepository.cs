using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.SeatManagement;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeSeatBookingRepository : ISeatBookingRepository, IEnumerable<ISeatBooking>
	{
		private readonly IList<ISeatBooking> _seatBookings = new List<ISeatBooking>();

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
			return _seatBookings.Count();
		}

		public void AddRange (IEnumerable<ISeatBooking> entityCollection)
		{
			entityCollection.ForEach (Add);
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		
		public ISeatBooking LoadAggregate (Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<ISeatBooking> LoadSeatBookingsForDateOnlyPeriod (DateOnlyPeriod period)
		{
			return _seatBookings
				.Where(booking => (booking.BelongsToDate >= period.StartDate
					&& booking.EndDateTime.Date <= period.EndDate.Date)).ToList();
		}

		public ISeatBooking LoadSeatBookingForPerson (DateOnly date, IPerson person)
		{
			return _seatBookings.SingleOrDefault(booking => (booking.BelongsToDate == date && booking.Person == person));
		}

		public IList<ISeatBooking> LoadSeatBookingsForDay (DateOnly date)
		{
			return _seatBookings
				.Where(booking => (booking.BelongsToDate == date)).ToList();
		}

		public void RemoveSeatBookingsForSeats (IEnumerable<ISeat> seats)
		{
			seats.ForEach(RemoveSeatBookingsForSeat);
		}

		public void RemoveSeatBookingsForSeat (ISeat seat)
		{
			GetSeatBookingsForSeat (seat)
				.ForEach(booking => _seatBookings.Remove(booking));
		}

		public IList<ISeatBooking> GetSeatBookingsForSeat (ISeat seat)
		{
			return _seatBookings.Where(booking => booking.Seat == seat).ToList();
		}

		public ISeatBookingReportModel LoadSeatBookingsReport (ISeatBookingReportCriteria criteria, Paging paging)
		{
			
			var seatBookings = LoadSeatBookingsForDateOnlyPeriod ( criteria.Period);
			var bookingModels = (from booking in seatBookings
								 where criteria.Locations.Contains(booking.Seat.Parent) &&
									   criteria.Teams.Contains(booking.Person.MyTeam(booking.BelongsToDate))
								 select booking);
			if (paging != null)
			{
				bookingModels = bookingModels.Take(paging.Take).Skip(paging.Skip);
			}

			var personSchedulesWithSeatBookings = new List<IPersonScheduleWithSeatBooking>();

			foreach (var booking in bookingModels)
			{
				var personScheduleWithSeatBooking = mapBookingToScheduleWithSeatBooking(booking);
				personSchedulesWithSeatBookings.Add (personScheduleWithSeatBooking);
			}


			return new SeatBookingReportModel()
			{
				RecordCount = bookingModels.Count(),
				SeatBookings = personSchedulesWithSeatBookings
			};

		}

		public IList<ISeatBooking> LoadSeatBookingsIntersectingDay (DateOnly dateOnly, ISeatMapLocation location)
		{
			var requestedDate = new DateTimePeriod(dateOnly.Date, dateOnly.Date.AddDays(1).AddSeconds(-1));
			return _seatBookings
				.Where(booking => !((requestedDate.EndDateTime < booking.StartDateTime) || (requestedDate.StartDateTime > booking.EndDateTime))
					&& location == booking.Seat.Parent)
				.ToList();
		}

		private static PersonScheduleWithSeatBooking mapBookingToScheduleWithSeatBooking (ISeatBooking booking)
		{
			var team = booking.Person.MyTeam (booking.BelongsToDate);
			var personScheduleWithSeatBooking = new PersonScheduleWithSeatBooking()
			{
				BelongsToDate = booking.BelongsToDate,
				FirstName = booking.Person.Name.FirstName,
				LastName = booking.Person.Name.LastName,
				LocationId = booking.Seat.Parent.Id.GetValueOrDefault(),
				LocationName = ((SeatMapLocation) booking.Seat.Parent).Name,
				PersonId = booking.Person.Id.GetValueOrDefault(),
				SeatBookingStart = booking.StartDateTime,
				SeatBookingEnd = booking.EndDateTime,
				SeatId = booking.Seat.Id.GetValueOrDefault(),
				SeatName = booking.Seat.Name,
				TeamId = team.Id.GetValueOrDefault(),
				TeamName = team.Description.Name
			};
			return personScheduleWithSeatBooking;
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