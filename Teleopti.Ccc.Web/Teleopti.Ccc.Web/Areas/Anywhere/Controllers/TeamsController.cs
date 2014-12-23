using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class TeamsController : Controller
	{
		private readonly IGetAdherence _getAdherence;
		private readonly ITeamAdherencePersister _teamAdherencePersister;
		private readonly ISiteAdherencePersister _siteAdherencePersister;

		public TeamsController(IGetAdherence getAdherence, ITeamAdherencePersister teamAdherencePersister, ISiteAdherencePersister siteAdherencePersister)
		{
			_getAdherence = getAdherence;
			_teamAdherencePersister = teamAdherencePersister;
			_siteAdherencePersister = siteAdherencePersister;
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

		[ReadModelUnitOfWork, HttpGet]
		public virtual int PollAdherenceForTeam(Guid teamId)
		{
			return _teamAdherencePersister.Get(teamId).AgentsOutOfAdherence;
		}

		[ReadModelUnitOfWork, HttpGet]
		public virtual int PollAdherenceForSite(Guid siteId)
		{
			return _siteAdherencePersister.Get(siteId).AgentsOutOfAdherence;
		}
	}
}