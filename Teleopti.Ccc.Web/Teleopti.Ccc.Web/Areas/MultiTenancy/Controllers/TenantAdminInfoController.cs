using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Controllers
{
	public class TenantAdminInfoController : Controller
    {
		private readonly ILoadAllTenantsUsers _loadAllTenantsUsers;

		public TenantAdminInfoController(ILoadAllTenantsUsers loadAllTenantsUsers)
		{
			_loadAllTenantsUsers = loadAllTenantsUsers;
		}

        public ActionResult Index()
		{
			//if we browse here directly we don't want to display it if we have admins
			if (_loadAllTenantsUsers.TenantUsers().Any())
				return Redirect(Url.Content("~/"));

			return View();
        }
    }
}