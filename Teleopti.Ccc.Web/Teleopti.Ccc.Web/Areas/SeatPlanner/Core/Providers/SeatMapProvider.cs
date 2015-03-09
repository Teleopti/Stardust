using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public class SeatMapProvider : ISeatMapProvider
	{
		private readonly ISeatMapRepository _seatMapRepository;

		protected SeatMapProvider()
		{
		}

		public SeatMapProvider(ISeatMapRepository seatMapRepository)
		{
			_seatMapRepository = seatMapRepository;
		}

		public SeatMapViewModel Get(Guid? id)
		{
			SeatMapViewModel seatMapVm = null;

			var seatMap = id.HasValue
				? _seatMapRepository.LoadAggregate(id.Value) as SeatMap
				: _seatMapRepository.LoadRootSeatMap() as SeatMap;

			if (seatMap != null)
			{
				//RobTodo: this is a quick hack, perhaps need a parent seat map reference on seat map
				var parentLocation = seatMap.Location.ParentLocation;
				var parentId = parentLocation != null ? parentLocation.Parent.Id : null;

				seatMapVm = new SeatMapViewModel()
				{
					Id = seatMap.Id.Value,
					ParentId = parentId != null ? parentId.Value : Guid.Empty,
					SeatMapJsonData = seatMap.SeatMapJsonData
				};

				buildBreadCrumbInformation(seatMapVm, seatMap.Location);

				seatMapVm.Location = new LocationViewModel()
				{
					Id = seatMap.Location.Id.Value,
					Name = seatMap.Location.Name
				};

			}
			return seatMapVm;
		}

		private static void buildBreadCrumbInformation(SeatMapViewModel seatMapVm, Location location)
		{
			if (location != null)
			{
				var breadcrumbList = new List<SeatMapBreadcrumbInfo>()
				{
					new SeatMapBreadcrumbInfo()
					{
						Id = location.Parent.Id.Value,
						Name = location.Name
					}
				};

				var parentLocation = location.ParentLocation;

				while (parentLocation != null)
				{
					breadcrumbList.Add(new SeatMapBreadcrumbInfo(){
											Id = parentLocation.Parent.Id.Value,
											Name = parentLocation.Name
										});

					parentLocation = parentLocation.ParentLocation;
				}

				seatMapVm.BreadcrumbInfo = Enumerable.Reverse(breadcrumbList);
			}
		}
	}
}