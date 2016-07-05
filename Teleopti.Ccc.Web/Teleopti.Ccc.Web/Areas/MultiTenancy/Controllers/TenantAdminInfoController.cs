using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

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
	        string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority.TrimEnd('/') + "/Administration";
			//string app = Request.ApplicationPath.TrimEnd('/') + "/Administration";
			//if we browse here directly we don't want to display it if we have admins
			if (_loadAllTenantsUsers.TenantUsers().Any())
				return Redirect(Url.Content("~/"));

			return  View(new AdminSiteInfoModel {UrlToAdminSite = baseUrl});

        }
    }
}