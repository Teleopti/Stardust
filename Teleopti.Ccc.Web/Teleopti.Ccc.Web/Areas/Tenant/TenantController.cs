using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;
using Teleopti.Ccc.Web.Areas.Tenant.Core;

namespace Teleopti.Ccc.Web.Areas.Tenant
{
	public class TenantController : Controller
	{
		private readonly IApplicationAuthentication _applicationAuthentication;
		private readonly IIdentityAuthentication _identityAuthentication;
		
		
		public TenantController(IApplicationAuthentication applicationAuthentication, IIdentityAuthentication identityAuthentication)
		{
			_applicationAuthentication = applicationAuthentication;
			_identityAuthentication = identityAuthentication;
		}

		[HttpGet]
		[TenantUnitOfWork]
		public virtual JsonResult ApplicationLogon(string userName, string password)
		{
			var res = _applicationAuthentication.Logon(userName, password);
			return Json(res, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[TenantUnitOfWork]
		public virtual JsonResult IdentityLogon(string identity)
		{
			var res = _identityAuthentication.Logon(identity);
			return Json(res, JsonRequestBehavior.AllowGet);
		}
	}
}