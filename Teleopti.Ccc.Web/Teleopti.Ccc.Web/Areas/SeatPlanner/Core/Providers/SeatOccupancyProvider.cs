using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class SeatOccupancyProvider : ISeatOccupancyProvider
	{
		private readonly ISeatBookingRepository _seatBookingRepository;
		private readonly ISeatMapLocationRepository _locationRepository;
		private readonly IUserTimeZone _userTimeZone;

		public SeatOccupancyProvider(ISeatBookingRepository seatBookingRepository, ISeatMapLocationRepository locationRepository, IUserTimeZone userTimeZone)
		{
			_seatBookingRepository = seatBookingRepository;
			_locationRepository = locationRepository;
			_userTimeZone = userTimeZone;
		}

		public IList<GroupedOccupancyViewModel> Get(IList<Guid> seatIds, DateOnly date)
		{

			var dateTimePeriodUtc = SeatManagementProviderUtils.GetUtcDateTimePeriodForLocalFullDay(date, _userTimeZone.TimeZone());

			var bookings = _seatBookingRepository.LoadSeatBookingsIntersectingDateTimePeriod(dateTimePeriodUtc, seatIds);
			return createGroupedViewModelsForBookings(bookings);
		}


		public IList<OccupancyViewModel> GetSeatBookingsForScheduleDays(IEnumerable<IScheduleDay> scheduleDays)
		{
			var bookings = new List<ISeatBooking>();

			foreach (var day in scheduleDays)
			{
				var personAssignment = day.PersonAssignment();
				if (personAssignment == null) continue;

				var bookingForPersonOnDate = _seatBookingRepository.LoadSeatBookingForPerson(personAssignment.Date, day.Person);
				if (bookingForPersonOnDate != null)
				{
					bookings.Add(bookingForPersonOnDate);
				}
			}

			return createViewModelsForBookings(bookings);
		}

		private IList<GroupedOccupancyViewModel> createGroupedViewModelsForBookings(IEnumerable<ISeatBooking> bookings)
		{
			var groupedBookings = bookings.GroupBy(booking => booking.Seat);
			var groupedOccupancyVieModelList = groupedBookings.Select(groupedBooking => new GroupedOccupancyViewModel()
			{
				SeatId = groupedBooking.Key.Id.GetValueOrDefault(),
				SeatName = groupedBooking.Key.Name,
				Occupancies = createViewModelsForBookings(groupedBooking)
			});


			return groupedOccupancyVieModelList.OrderBy(groupedBooking => groupedBooking.SeatName).ToList();
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
				LocationPath = location != null ? SeatMapProvider.GetLocationPath(location, true) : null,
				LocationPrefix = location?.LocationPrefix,
				LocationSuffix = location?.LocationSuffix
			};
		}

		private DateTime convertTimeToLocal(DateTime dateTime)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(dateTime, _userTimeZone.TimeZone());
		}


	}

	public class GroupedOccupancyViewModel
	{
		private List<OccupancyViewModel> _occupancies = new List<OccupancyViewModel>();
		public Guid SeatId { get; set; }
		public String SeatName { get; set; }

		public List<OccupancyViewModel> Occupancies
		{
			get { return _occupancies; }
			set { _occupancies = value; }
		}
	}

}