using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.SeatManagement;


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

		public IEnumerable<ISeatBooking> LoadAll()
		{
			return _seatBookings;
		}

		public ISeatBooking Load (Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			return _seatBookings.Count;
		}

		public IList<ISeatBooking> LoadSeatBookingsForDateOnlyPeriod (DateOnlyPeriod period)
		{
			return _seatBookings
				.Where(booking => booking.BelongsToDate >= period.StartDate
								  && booking.BelongsToDate <= period.EndDate).ToList();
		}
		
		public IList<ISeatBooking> LoadSeatBookingsForPerson (DateOnlyPeriod period, IPerson person)
		{
			return _seatBookings.Where(booking => booking.BelongsToDate >= period.StartDate
															&& booking.BelongsToDate <= period.EndDate && booking.Person == person).ToList();
		}

		public IList<ISeatBooking> LoadSeatBookingsForDay (DateOnly date)
		{
			return _seatBookings
				.Where(booking => booking.BelongsToDate == date).ToList();
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
			if (!paging.Equals(Paging.Empty))
			{
				bookingModels = bookingModels.Take(paging.Take).Skip(paging.Skip);
			}

			var personSchedulesWithSeatBookings = new List<IPersonScheduleWithSeatBooking>();

			foreach (var booking in bookingModels)
			{
				var personScheduleWithSeatBooking = mapBookingToScheduleWithSeatBooking(booking);
				personSchedulesWithSeatBookings.Add (personScheduleWithSeatBooking);
			}
			
			return new SeatBookingReportModel
			{
				RecordCount = bookingModels.Count(),
				SeatBookings = personSchedulesWithSeatBookings
			};

		}

		public IList<ISeatBooking> LoadSeatBookingsIntersectingDateTimePeriod(DateTimePeriod dateTimePeriod, Guid locationId)
		{
			return _seatBookings
				.Where(booking => !((dateTimePeriod.EndDateTime < booking.StartDateTime) || (dateTimePeriod.StartDateTime > booking.EndDateTime))
					&& locationId == booking.Seat.Parent.Id)
				.ToList();
		}

		public IList<ISeatBooking> LoadSeatBookingsIntersectingDateTimePeriod(DateTimePeriod dateTimePeriod, IList<Guid> seatIds)
		{
			return _seatBookings
				.Where(booking => !((dateTimePeriod.EndDateTime < booking.StartDateTime) || (dateTimePeriod.StartDateTime > booking.EndDateTime))
					&& seatIds.Contains (booking.Seat.Id.GetValueOrDefault()))
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
				LocationPrefix = ((SeatMapLocation) booking.Seat.Parent).LocationPrefix,
				LocationSuffix = ((SeatMapLocation) booking.Seat.Parent).LocationSuffix,
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