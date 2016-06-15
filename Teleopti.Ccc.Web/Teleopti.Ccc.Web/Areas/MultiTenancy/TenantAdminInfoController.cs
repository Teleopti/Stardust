using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	[RouteArea("MultiTenancy")]
	public class TenantAdminInfoController : Controller
    {
		private readonly ILoadAllTenantsUsers _loadAllTenantsUsers;

		public TenantAdminInfoController(ILoadAllTenantsUsers loadAllTenantsUsers)
		{
			_loadAllTenantsUsers = loadAllTenantsUsers;
		}

		[Route("TenantAdminInfo")]
        public ActionResult Index()
		{
			//if we browse here directly we don't want to display it if we have admins
			if (_loadAllTenantsUsers.TenantUsers().Any())
				return Redirect(Url.Content("~/"));

			return View();
        }
    }
}