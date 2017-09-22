using System.Web.Mvc;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

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
	        string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;
			string app = Request.ApplicationPath.TrimEnd('/').RemoveFromEnd("/Web") + "/Administration";
			//if we browse here directly we don't want to display it if we have admins
			if (_checkTenantUserExists.Exists())
				return Redirect(Url.Content("~/"));

			return  View(new AdminSiteInfoModel {UrlToAdminSite = baseUrl + app});

        }
    }

	
}