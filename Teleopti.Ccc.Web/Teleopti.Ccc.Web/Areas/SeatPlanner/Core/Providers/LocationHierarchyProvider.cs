using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class LocationHierarchyProvider : ILocationHierarchyProvider
	{
		private readonly ISeatMapLocationRepository _seatMapLocationRepository;

		public LocationHierarchyProvider(ISeatMapLocationRepository seatMapLocationRepository)
		{
			_seatMapLocationRepository = seatMapLocationRepository;
		}

		public LocationViewModel Get()
		{

			LocationViewModel locationViewModel = null;
			var rootSeatMapLocation = _seatMapLocationRepository.LoadRootSeatMap() as SeatMapLocation;

			if (rootSeatMapLocation != null)
			{
				locationViewModel = getLocationViewModel(rootSeatMapLocation);
			}
			return locationViewModel;
		}

		private static LocationViewModel getLocationViewModel(SeatMapLocation location)
		{
			if (location == null)
			{
				return null;
			}

			var childViewModels = getChildViewModels(location);
			var seatViewModels = getSeatViewModels(location);

			var locationViewModel = new LocationViewModel()
			{
				Id = Guid.Parse(location.Id.ToString()),
				Name = location.Name,
				LocationPrefix = location.LocationPrefix,
				LocationSuffix = location.LocationSuffix,
				Seats = seatViewModels,
				Children = childViewModels
			};

			return locationViewModel;
		}

		private static List<LocationViewModel> getChildViewModels(SeatMapLocation location)
		{
			if (location.ChildLocations != null)
			{
				var childLocations = location.ChildLocations.OrderBy(childLocation => childLocation.Name);
				return childLocations.Select(getLocationViewModel).ToList();
			}

			return null;
		}

		private static List<SeatViewModel> getSeatViewModels(ISeatMapLocation location)
		{
			if (location.Seats != null)
			{
				var seatViewModels = new List<SeatViewModel>();
				foreach (var seat in location.Seats)
				{
					var seatViewModel = new SeatViewModel() { Id = seat.Id.Value, Name = seat.Name };
					seatViewModels.Add(seatViewModel);
				}
				return seatViewModels;
			}

			return null;
		}
	}
}

