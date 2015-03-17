using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class LocationViewModel
	{
		public Guid Id { get; set; }
		public Guid ParentId;
		public String Name { get; set; }
		public List<LocationViewModel> Children{ get; set; }
		public List<SeatViewModel> Seats { get; set; }
		public String SeatMapJsonData;
		public IEnumerable<SeatMapLocationBreadcrumbInfo> BreadcrumbInfo;
	}

	public class SeatMapLocationBreadcrumbInfo
	{
		public Guid Id;
		public String Name;

	}
}