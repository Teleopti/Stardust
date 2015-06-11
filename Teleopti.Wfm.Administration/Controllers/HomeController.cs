using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.Controllers
{
	public class HomeController : Controller
	{
		private readonly ITenantList _tenantList;

		public HomeController(ITenantList tenantList)
		{
			_tenantList = tenantList;
		}

		// GET: Home
		public ActionResult Index()
		{
			return View();
		}

		[HttpGet]
		[TenantUnitOfWork]
		public JsonResult GetAllTenants()
		{
			return Json(_tenantList.GetTenantList(),JsonRequestBehavior.AllowGet);
		}
	}
}