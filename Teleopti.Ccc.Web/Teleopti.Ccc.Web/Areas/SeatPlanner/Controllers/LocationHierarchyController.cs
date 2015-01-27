using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers
{
	public class LocationHierarchyController : Controller
	{
		private readonly ILocationHierarchyProvider _locationHierarchyProvider;

	   public LocationHierarchyController(ILocationHierarchyProvider locationHierarchyProvider)
		{
			_locationHierarchyProvider = locationHierarchyProvider;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult Get()
		{
			var path = Request.MapPath("~/Areas/SeatPlanner/Content/Temp/Locations.txt");
			return Json(_locationHierarchyProvider.Get(path), JsonRequestBehavior.AllowGet);
		}
	}
}


