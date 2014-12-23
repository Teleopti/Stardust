using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class TeamsController : Controller
	{
		private readonly IGetAdherence _getAdherence;

		public TeamsController(IGetAdherence getAdherence)
		{
			_getAdherence = getAdherence;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult ForSite(string siteId)
		{
			return Json(_getAdherence.ForSite(siteId), JsonRequestBehavior.AllowGet);
		}

	
		[UnitOfWorkAction, HttpGet]
		public JsonResult GetOutOfAdherence(string teamId)
		{
			return Json(_getAdherence.GetOutOfAdherence(teamId), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction, HttpGet]
		public virtual JsonResult GetOutOfAdherenceForTeamsOnSite(string siteId)
		{
			return Json(_getAdherence.GetOutOfAdherenceForTeamsOnSite(siteId), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult GetBusinessUnitId(string teamId)
		{
			return Json(_getAdherence.GetBusinessUnitId(teamId), JsonRequestBehavior.AllowGet);
		}
	}
}