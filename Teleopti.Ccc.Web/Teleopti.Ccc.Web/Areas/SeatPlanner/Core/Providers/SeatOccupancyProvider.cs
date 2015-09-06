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
		private readonly IUserTimeZone _userTimeZone;

		public SeatOccupancyProvider(ISeatBookingRepository seatBookingRepository, IUserTimeZone userTimeZone)
		{
			_seatBookingRepository = seatBookingRepository;
			_userTimeZone = userTimeZone;
		}

		public IList<OccupancyViewModel> Get(Guid seatId, DateOnly date)
		{
			var bookings = _seatBookingRepository.LoadSeatBookingsForSeatIntersectingDay(date, seatId);
			return createViewModelsForBookings(bookings);
		}

		private List<OccupancyViewModel> createViewModelsForBookings(IEnumerable<ISeatBooking> bookings)
		{
			return bookings.Select(createOccupancyViewModel).ToList();
			
		}

		private OccupancyViewModel createOccupancyViewModel(ISeatBooking booking)
		{
			return new OccupancyViewModel()
			{
				StartDateTime = convertTimeToLocal(booking.StartDateTime),
				EndDateTime = convertTimeToLocal(booking.EndDateTime),
				BelongsToDate = booking.BelongsToDate,
				PersonId = booking.Person.Id.GetValueOrDefault(),
				FirstName = booking.Person.Name.FirstName,
				LastName = booking.Person.Name.LastName,
				SeatId = booking.Seat.Id.GetValueOrDefault(),
				SeatName = booking.Seat.Name,
				BookingId = booking.Id.GetValueOrDefault()
			};
		}


		private DateTime convertTimeToLocal(DateTime dateTime)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(dateTime, _userTimeZone.TimeZone());

		}

	}
}