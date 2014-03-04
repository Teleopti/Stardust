using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class TeamsController : Controller
	{
		private readonly ISiteRepository _siteRepository;

		public TeamsController(ISiteRepository siteRepository)
		{
			_siteRepository = siteRepository;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult ForSite(string siteId)
		{
			return Json(
				_siteRepository.Get(new Guid(siteId))
				               .TeamCollection
											 .Select(teamViewModel => new TeamViewModel { Name = teamViewModel.Description.Name }), JsonRequestBehavior.AllowGet
				);
		}
	}
}