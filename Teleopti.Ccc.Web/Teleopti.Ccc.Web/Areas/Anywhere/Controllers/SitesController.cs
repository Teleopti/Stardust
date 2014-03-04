using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class SitesController : Controller
	{
		private readonly ISiteRepository _siteRepository;

		public SitesController(ISiteRepository siteRepository)
		{
			_siteRepository = siteRepository;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult Index()
		{
			return Json(_siteRepository.LoadAll().Select(x =>
			            new SiteViewModel
				            {
					            Id = x.Id.Value.ToString(),
					            Name = x.Description.Name
				            }), JsonRequestBehavior.AllowGet);
		}
	}
}