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
		private readonly ISeatMapLocationRepository _locationRepository;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ILoggedOnUser _loggedOnUser;

		public SeatOccupancyProvider(ISeatBookingRepository seatBookingRepository, ISeatMapLocationRepository locationRepository, IUserTimeZone userTimeZone, ILoggedOnUser loggedOnUser)
		{
			_seatBookingRepository = seatBookingRepository;
			_locationRepository = locationRepository;
			_userTimeZone = userTimeZone;
			_loggedOnUser = loggedOnUser;
		}

		public IList<OccupancyViewModel> Get(Guid seatId, DateOnly date)
		{
			var bookings = _seatBookingRepository.LoadSeatBookingsForSeatIntersectingDay(date, seatId);
			return createViewModelsForBookings(bookings);
		}

		public IList<OccupancyViewModel> GetSeatBookingsForCurrentUser (DateOnlyPeriod dateOnlyPeriod)
		{
			return getSeatBookingsForPerson (_loggedOnUser.CurrentUser(), dateOnlyPeriod);
		}

		private List<OccupancyViewModel> createViewModelsForBookings(IEnumerable<ISeatBooking> bookings)
		{
			return bookings.Select(createOccupancyViewModel).ToList();
		}

		private OccupancyViewModel createOccupancyViewModel(ISeatBooking booking)
		{

			var location = _locationRepository.Get(booking.Seat.Parent.Id.GetValueOrDefault());

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
				BookingId = booking.Id.GetValueOrDefault(),
				LocationPath = location != null ? SeatMapProvider.GetLocationPath(location, true) : null
			};
		}
		
		private DateTime convertTimeToLocal(DateTime dateTime)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(dateTime, _userTimeZone.TimeZone());
		}

		private List<OccupancyViewModel> getSeatBookingsForPerson (IPerson person, DateOnlyPeriod bookingDatePeriod)
		{
			var bookings = _seatBookingRepository.LoadSeatBookingsForDateOnlyPeriod (person, bookingDatePeriod);
			return createViewModelsForBookings (bookings);
		}
	}
}