using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class SeatMapProvider : ISeatMapProvider
	{
		private readonly ISeatMapLocationRepository _seatMapLocationRepository;
		private readonly ISeatBookingRepository _seatMapBookingRepository;

		protected SeatMapProvider()
		{
		}

		public SeatMapProvider(ISeatMapLocationRepository seatMapLocationRepository, ISeatBookingRepository seatMapBookingRepository)
		{
			_seatMapLocationRepository = seatMapLocationRepository;
			_seatMapBookingRepository = seatMapBookingRepository;
		}


		public LocationViewModel Get(Guid? id)
		{
			var seatMapLocation = getSeatMapLocation(id);
			return seatMapLocation != null ? buildLocationViewModel(seatMapLocation, false) : null;
		}

		public LocationViewModel Get(Guid? id, DateOnly bookingDate)
		{
			var seatMapLocation = getSeatMapLocation(id);
			return seatMapLocation != null ? buildLocationViewModel(seatMapLocation, true) : null;
		
		}

		private SeatMapLocation getSeatMapLocation(Guid? id)
		{
			var seatMapLocation = id.HasValue
				? _seatMapLocationRepository.LoadAggregate(id.Value) as SeatMapLocation
				: _seatMapLocationRepository.LoadRootSeatMap() as SeatMapLocation;
			return seatMapLocation;
		}

		private static LocationViewModel buildLocationViewModel (SeatMapLocation seatMapLocation, bool includeSeatAndOccupancy)
		{
			var parentLocation = seatMapLocation.ParentLocation;
			var parentId = parentLocation != null ? parentLocation.Id : null;

			var locationVM = new LocationViewModel()
			{
				Id = seatMapLocation.Id.Value,
				Name = seatMapLocation.Name,
				ParentId = parentId != null ? parentId.Value : Guid.Empty,
				SeatMapJsonData = seatMapLocation.SeatMapJsonData
			};

			if (includeSeatAndOccupancy)
			{
				locationVM.Seats = getSeatViewModels (seatMapLocation);
			}
			
			buildBreadCrumbInformation (locationVM, seatMapLocation);
			return locationVM;
		}

		private static List<SeatViewModel> getSeatViewModels(ISeatMapLocation location)
		{
			if (location.Seats != null)
			{
				return location.Seats.Select (seat => new SeatViewModel()
				{
					Id = seat.Id.Value, Name = seat.Name
				}).ToList();
			}

			return null;
		}
		
		private static void buildBreadCrumbInformation(LocationViewModel seatMapLocationViewModel, SeatMapLocation seatMapLocation)
		{
			if (seatMapLocation == null) return;

			seatMapLocationViewModel.BreadcrumbInfo = buildBreadCrumbList(seatMapLocation);
		}

		public static String GetLocationPath (ISeatMapLocation seatMapLocation)
		{
			return String.Join ("/", buildBreadCrumbList (seatMapLocation as SeatMapLocation).Select (breadCrumb => breadCrumb.Name));
		}

		private static IEnumerable<SeatMapLocationBreadcrumbInfo> buildBreadCrumbList (SeatMapLocation seatMapLocation)
		{
			var breadcrumbList = new List<SeatMapLocationBreadcrumbInfo>()
			{
				new SeatMapLocationBreadcrumbInfo()
				{
					Id = seatMapLocation.Id.Value,
					Name = seatMapLocation.Name
				}
			};

			var parentLocation = seatMapLocation.ParentLocation;

			while (parentLocation != null)
			{
				breadcrumbList.Add (new SeatMapLocationBreadcrumbInfo()
				{
					Id = parentLocation.Id.Value,
					Name = parentLocation.Name
				});

				parentLocation = parentLocation.ParentLocation;
			}
			return Enumerable.Reverse(breadcrumbList);
		}
	}
}