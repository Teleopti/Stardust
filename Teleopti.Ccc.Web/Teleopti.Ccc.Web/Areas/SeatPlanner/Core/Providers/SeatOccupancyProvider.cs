using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class SeatOccupancyProvider : ISeatOccupancyProvider
	{
		private readonly ISeatBookingRepository _seatBookingRepository;

		public SeatOccupancyProvider(ISeatBookingRepository seatBookingRepository)
		{
			_seatBookingRepository = seatBookingRepository;
		}

		public IList<OccupancyViewModel> Get(Guid seatId, DateOnly date)
		{
			var bookings = _seatBookingRepository.LoadSeatBookingsForSeatIntersectingDay(date, seatId);
			return createViewModelsForBookings(bookings);
		}

		private static List<OccupancyViewModel> createViewModelsForBookings(IEnumerable<ISeatBooking> bookings)
		{
			return bookings.Select(createOccupancyViewModel).ToList();
			
		}

		private static OccupancyViewModel createOccupancyViewModel(ISeatBooking booking)
		{
			return new OccupancyViewModel()
			{
				StartDateTime = booking.StartDateTime,
				EndDateTime = booking.EndDateTime,
				PersonId = booking.Person.Id.GetValueOrDefault(),
				FirstName = booking.Person.Name.FirstName,
				LastName = booking.Person.Name.LastName,
				SeatId = booking.Seat.Id.GetValueOrDefault(),
				BookingId = booking.Id.GetValueOrDefault()
			};
		}
	}
}