using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class SeatMapViewModel
	{
		public Guid Id;
		public Guid ParentId;
		public String Name;
		public LocationViewModel Location;
		public String SeatMapJsonData;
		public IEnumerable<SeatMapBreadcrumbInfo> BreadcrumbInfo;
	}

	public class SeatMapBreadcrumbInfo
	{
		public Guid Id;
		public String Name;
		
	}

	
}