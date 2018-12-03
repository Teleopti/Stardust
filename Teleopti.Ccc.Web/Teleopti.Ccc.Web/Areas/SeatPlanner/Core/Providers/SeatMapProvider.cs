using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class SeatMapProvider : ISeatMapProvider
	{
		private readonly ISeatMapLocationRepository _seatMapLocationRepository;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ISeatBookingRepository _seatMapBookingRepository;

		protected SeatMapProvider()
		{
		}

		public SeatMapProvider(ISeatMapLocationRepository seatMapLocationRepository, ISeatBookingRepository seatMapBookingRepository, IUserTimeZone userTimeZone)
		{
			_seatMapLocationRepository = seatMapLocationRepository;
			_seatMapBookingRepository = seatMapBookingRepository;
			_userTimeZone = userTimeZone;
		}

		public LocationViewModel Get(Guid? id, DateOnly? bookingDate = null)
		{
			var seatMapLocation = getSeatMapLocation(id);
			if (seatMapLocation == null) return null;

			var locationViewModel = buildLocationViewModel (seatMapLocation, bookingDate);
			return locationViewModel;
		}

		private SeatMapLocation getSeatMapLocation(Guid? id)
		{
			return id.HasValue
				? _seatMapLocationRepository.LoadAggregate(id.Value) as SeatMapLocation
				: _seatMapLocationRepository.LoadRootSeatMap() as SeatMapLocation;
		}

		private LocationViewModel buildLocationViewModel (SeatMapLocation seatMapLocation, DateOnly? bookingDate)
		{
			var parentLocation = seatMapLocation.ParentLocation;
			var parentId = parentLocation != null ? parentLocation.Id : null;
			var bookings = getOccupancyInformation(seatMapLocation, bookingDate);

			var locationVM = new LocationViewModel()
			{
				Id = seatMapLocation.Id.Value,
				Name = seatMapLocation.Name,
				LocationPrefix = seatMapLocation.LocationPrefix,
				LocationSuffix = seatMapLocation.LocationSuffix,
				ParentId = parentId != null ? parentId.Value : Guid.Empty,
				SeatMapJsonData = seatMapLocation.SeatMapJsonData,
				Seats = buildSeatViewModels(seatMapLocation, bookings)
			};

			buildBreadCrumbInformation (locationVM, seatMapLocation);
			return locationVM;
		}

		private IList<ISeatBooking> getOccupancyInformation(ISeatMapLocation seatMapLocation, DateOnly? bookingDate)
		{
			if (bookingDate == null) return null;

			var dateTimePeriodUtc = SeatManagementProviderUtils.GetUtcDateTimePeriodForLocalFullDay(bookingDate.Value, _userTimeZone.TimeZone());
			return _seatMapBookingRepository
				.LoadSeatBookingsIntersectingDateTimePeriod(dateTimePeriodUtc, seatMapLocation.Id.GetValueOrDefault());
		}

		private static List<SeatViewModel> buildSeatViewModels(ISeatMapLocation location, IEnumerable<ISeatBooking> bookings)
		{
			if (location.Seats != null)
			{
				return location.Seats.Select (seat =>
				{
					var roles = seat.Roles
									.Select(role => (ApplicationRole) role)
									.Where(role => !role.IsDeleted)
									.Select(role => role.Id.Value)
									.ToList();
					
					return new SeatViewModel()
					{
						Id = seat.Id.Value,
						Name = seat.Name,
						IsOccupied = bookings!=null && bookings.Any(booking => booking.Seat.Id == seat.Id),
						RoleIdList = roles,
						Priority = seat.Priority
					};

				}).ToList();
			}

			return null;
		}

		private static void buildBreadCrumbInformation(LocationViewModel seatMapLocationViewModel, SeatMapLocation seatMapLocation, bool hideRootLocation = false)
		{
			if (seatMapLocation == null) return;

			seatMapLocationViewModel.BreadcrumbInfo = buildBreadCrumbList(seatMapLocation, hideRootLocation);
		}

		public static String GetLocationPath (ISeatMapLocation seatMapLocation, bool hideRootLocation = false)
		{
			return String.Join ("/", buildBreadCrumbList (seatMapLocation as SeatMapLocation, hideRootLocation).Select (breadCrumb => breadCrumb.Name));
		}

		private static IEnumerable<SeatMapLocationBreadcrumbInfo> buildBreadCrumbList(SeatMapLocation seatMapLocation, bool hideRootLocation)
		{
			var breadcrumbList = new List<SeatMapLocationBreadcrumbInfo>();
			var parentLocation = seatMapLocation.ParentLocation;

			if (parentLocation == null && hideRootLocation)
			{
				return breadcrumbList;
			}

			breadcrumbList.Add (new SeatMapLocationBreadcrumbInfo()
			{
				Id = seatMapLocation.Id.Value,
				Name = seatMapLocation.Name
			});
		
			while (parentLocation != null)
			{

				var suppressRoot = (parentLocation.ParentLocation == null && hideRootLocation);

				if (!suppressRoot)
				{
					breadcrumbList.Add(new SeatMapLocationBreadcrumbInfo()
					{
						Id = parentLocation.Id.Value,
						Name = parentLocation.Name
					});
				}

				parentLocation = parentLocation.ParentLocation;

			}
			return Enumerable.Reverse(breadcrumbList);
		}
	}
}