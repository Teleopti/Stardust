using System.Web.Mvc;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.Web.Auth;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Controllers
{
	public class TenantAdminInfoController : Controller
    {
		private readonly ICheckTenantUserExists _checkTenantUserExists;

		public TenantAdminInfoController(ICheckTenantUserExists checkTenantUserExists)
		{
			_checkTenantUserExists = checkTenantUserExists;
		}

        public ActionResult Index()
        {
			var requestUrl = Request.UrlConsideringLoadBalancerHeaders();
			string baseUrl = requestUrl.Scheme + "://" + requestUrl.Authority;
			string app = Request.ApplicationPath.TrimEnd('/').RemoveFromEnd("/Web") + "/Administration";
			//if we browse here directly we don't want to display it if we have admins
			if (_checkTenantUserExists.Exists())
				return Redirect(Url.Content("~/"));

			return  View(new AdminSiteInfoModel {UrlToAdminSite = baseUrl + app});

        }
    }

	
}