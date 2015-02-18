﻿using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class TeamsController : Controller
	{
		private readonly IGetTeamAdherence _getTeamAdherence;
		private readonly IGetBusinessUnitId _getBusinessUnitId;

		public TeamsController(IGetTeamAdherence getTeamAdherence, IGetBusinessUnitId getBusinessUnitId)
		{
			_getTeamAdherence = getTeamAdherence;
			_getBusinessUnitId = getBusinessUnitId;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult ForSite(string siteId)
		{
			return Json(_getTeamAdherence.ForSite(siteId), JsonRequestBehavior.AllowGet);
		}


		[UnitOfWorkAction, HttpGet]
		public virtual JsonResult GetOutOfAdherence(string teamId)
		{
			return Json(_getTeamAdherence.GetOutOfAdherence(teamId), JsonRequestBehavior.AllowGet);
		}
		
		[ReadModelUnitOfWork, HttpGet]
		public virtual JsonResult GetOutOfAdherenceForTeamsOnSite(string siteId)
		{
			return Json(_getTeamAdherence.GetOutOfAdherenceForTeamsOnSite(siteId), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult GetBusinessUnitId(string teamId)
		{
			return Json(_getBusinessUnitId.Get(teamId), JsonRequestBehavior.AllowGet);
		}
	}
}