﻿using System;
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

		public LocationViewModel Get(Guid? id, DateOnly? bookingDate = null)
		{
			var seatMapLocation = getSeatMapLocation(id);
			if (seatMapLocation == null) return null;

			var locationViewModel = buildLocationViewModel (seatMapLocation);
			if (bookingDate != null)
			{
				addOccupancyInformation(seatMapLocation, locationViewModel, bookingDate.Value);
			
			}
			return locationViewModel;
		}

		private SeatMapLocation getSeatMapLocation(Guid? id)
		{
			return id.HasValue
				? _seatMapLocationRepository.LoadAggregate(id.Value) as SeatMapLocation
				: _seatMapLocationRepository.LoadRootSeatMap() as SeatMapLocation;
		}

		private static LocationViewModel buildLocationViewModel (SeatMapLocation seatMapLocation)
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

			buildBreadCrumbInformation (locationVM, seatMapLocation);
			return locationVM;
		}

		private void addOccupancyInformation (ISeatMapLocation seatMapLocation, LocationViewModel locationViewModel, DateOnly bookingDate)
		{
			var bookings = _seatMapBookingRepository.LoadSeatBookingsIntersectingDay (bookingDate, seatMapLocation.Id.GetValueOrDefault());
			if (bookings != null)
			{
				locationViewModel.Seats = buildSeatViewModels(seatMapLocation, bookings);	
			}
		}

		private static List<SeatViewModel> buildSeatViewModels(ISeatMapLocation location, IEnumerable<ISeatBooking> bookings )
		{
			if (location.Seats != null)
			{
				return location.Seats.Select (seat => new SeatViewModel()
				{
					Id = seat.Id.Value, 
					Name = seat.Name,
					IsOccupied = bookings.Any(booking => booking.Seat.Id == seat.Id)
					
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