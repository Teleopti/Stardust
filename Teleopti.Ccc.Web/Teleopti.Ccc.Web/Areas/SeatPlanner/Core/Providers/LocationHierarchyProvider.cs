using System;
using System.Collections.Generic;
using System.Web.Helpers;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class LocationHierarchyProvider : ILocationHierarchyProvider
	{
		public LocationHierarchyProvider()
		{

		}

		public LocationViewModel Get(string path)
		{

			LocationViewModel locationViewModel = null;

			var locationString = System.IO.File.ReadAllText(path);
			if (!String.IsNullOrEmpty(locationString))
			{
				var locationData = Json.Decode(locationString);
				dynamic rootLocation = locationData;
				locationViewModel = getLocationViewModel(rootLocation);
			}
			return locationViewModel;

		}

		private static LocationViewModel getLocationViewModel(dynamic location)
		{
			if (location == null)
			{
				return null;
			}

			var childViewModels = getChildViewModels(location);
			var seatViewModels = getSeatViewModels(location);

			var locationViewModel = new LocationViewModel()
			{
				Id = Guid.Parse (location.id),
				Name = location.name,
				Seats = seatViewModels,
				Children = childViewModels
			};

			return locationViewModel;
		}

		private static List<LocationViewModel> getChildViewModels(dynamic location)
		{
			if (location.childLocations != null)
			{
				var childViewModels = new List<LocationViewModel>();
				var childLocations = location.childLocations;
			
				foreach (var child in childLocations)
				{
					childViewModels.Add (getLocationViewModel (child));
				}

				return childViewModels;
			}

			return null;
		}

		private static List<SeatViewModel> getSeatViewModels (dynamic location)
		{
			if (location.seats != null)
			{
				var seatViewModels = new List<SeatViewModel>();
				foreach (var seat in location.seats)
				{
					var seatViewModel = new SeatViewModel() {Id = Guid.Parse(seat.id), Name = seat.name};
					seatViewModels.Add (seatViewModel);
				}
				return seatViewModels;
			}

			return null;
		}
	}
}

