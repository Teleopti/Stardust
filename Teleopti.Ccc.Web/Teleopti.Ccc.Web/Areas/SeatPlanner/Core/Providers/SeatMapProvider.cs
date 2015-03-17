using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class SeatMapProvider : ISeatMapProvider
	{
		private readonly ISeatMapLocationRepository _seatMapLocationRepository;

		protected SeatMapProvider()
		{
		}

		public SeatMapProvider(ISeatMapLocationRepository seatMapLocationRepository)
		{
			_seatMapLocationRepository = seatMapLocationRepository;
		}

		public LocationViewModel Get(Guid? id)
		{
			LocationViewModel locationVM = null;

			var seatMapLocation = id.HasValue
				? _seatMapLocationRepository.LoadAggregate(id.Value) as SeatMapLocation
				: _seatMapLocationRepository.LoadRootSeatMap() as SeatMapLocation;

			if (seatMapLocation != null)
			{
				var parentLocation = seatMapLocation.ParentLocation;
				var parentId = parentLocation != null ? parentLocation.Id : null;

				locationVM = new LocationViewModel()
				{
					Id = seatMapLocation.Id.Value,
					Name = seatMapLocation.Name,
					ParentId = parentId != null ? parentId.Value : Guid.Empty,
					SeatMapJsonData = seatMapLocation.SeatMapJsonData
				};

				buildBreadCrumbInformation(locationVM, seatMapLocation);

			}
			return locationVM;
		}

		private static void buildBreadCrumbInformation(LocationViewModel seatMapLocationViewModel, SeatMapLocation seatMapLocation)
		{
			if (seatMapLocation != null)
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
					breadcrumbList.Add(new SeatMapLocationBreadcrumbInfo(){
											Id = parentLocation.Id.Value,
											Name = parentLocation.Name
										});

					parentLocation = parentLocation.ParentLocation;
				}

				seatMapLocationViewModel.BreadcrumbInfo = Enumerable.Reverse(breadcrumbList);
			}
		}
	}
}