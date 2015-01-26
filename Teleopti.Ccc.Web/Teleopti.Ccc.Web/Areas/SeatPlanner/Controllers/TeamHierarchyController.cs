
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers
{
	public class TeamHierarchyController : Controller
	{
		private readonly ITeamsProvider _teamsProvider;

		public TeamHierarchyController(ITeamsProvider teamsProvider)
		{
			_teamsProvider = teamsProvider;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult Get()
		{
			return Json(_teamsProvider.GetTeamHierarchy(), JsonRequestBehavior.AllowGet);

		}


	}
}