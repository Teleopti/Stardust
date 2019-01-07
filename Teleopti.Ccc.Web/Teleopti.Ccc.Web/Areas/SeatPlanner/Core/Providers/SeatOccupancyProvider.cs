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

		public IList<OccupancyViewModel> GetSeatBookingsForScheduleDays(DateOnlyPeriod period, IPerson person)
		{
			var bookings = _seatBookingRepository.LoadSeatBookingsForPerson(period, person).GroupBy(b => b.Seat);
			return bookings.Select(createOccupancyViewModel).SelectMany(b => b).ToList();
		}

		private IList<GroupedOccupancyViewModel> createGroupedViewModelsForBookings(IEnumerable<ISeatBooking> bookings)
		{
			var groupedBookings = bookings.GroupBy(booking => booking.Seat);
			var groupedOccupancyVieModelList = groupedBookings.Select(groupedBooking => new GroupedOccupancyViewModel
			{
				SeatId = groupedBooking.Key.Id.GetValueOrDefault(),
				SeatName = groupedBooking.Key.Name,
				Occupancies = createOccupancyViewModel(groupedBooking)
			});
			
			return groupedOccupancyVieModelList.OrderBy(groupedBooking => groupedBooking.SeatName).ToList();
		}
		
		private List<OccupancyViewModel> createOccupancyViewModel(IGrouping<ISeat, ISeatBooking> booking)
		{
			var location = _locationRepository.Get(booking.Key.Parent.Id.GetValueOrDefault());
			var timeZone = _userTimeZone.TimeZone();

			return booking.Select(b => new OccupancyViewModel
			{
				StartDateTime = convertTimeToLocal(b.StartDateTime, timeZone),
				EndDateTime = convertTimeToLocal(b.EndDateTime, timeZone),
				BelongsToDate = b.BelongsToDate,
				PersonId = b.Person.Id.GetValueOrDefault(),
				FirstName = b.Person.Name.FirstName,
				LastName = b.Person.Name.LastName,
				SeatId = booking.Key.Id.GetValueOrDefault(),
				SeatName = booking.Key.Name,
				BookingId = b.Id.GetValueOrDefault(),
				LocationPath = location != null ? SeatMapProvider.GetLocationPath(location, true) : null,
				LocationPrefix = location?.LocationPrefix,
				LocationSuffix = location?.LocationSuffix
			}).ToList();
		}

		private DateTime convertTimeToLocal(DateTime dateTime, TimeZoneInfo timeZone)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZone);
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