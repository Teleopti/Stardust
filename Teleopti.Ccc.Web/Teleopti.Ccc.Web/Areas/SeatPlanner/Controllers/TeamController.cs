
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers
{
	public class TeamController : Controller
	{
		private readonly ITeamsProvider _teamsProvider;

		public TeamController(ITeamsProvider teamsProvider)
		{
			_teamsProvider = teamsProvider;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult Get(string siteId)
		{
			return Json(_teamsProvider.Get(siteId), JsonRequestBehavior.AllowGet);

		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult GetTeamHierarchy()
		{
			return Json(_teamsProvider.GetTeamHierarchy(), JsonRequestBehavior.AllowGet);

		}


	}
}