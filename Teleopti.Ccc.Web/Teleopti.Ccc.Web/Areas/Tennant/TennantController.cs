using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Web.Areas.Tennant.Core;

namespace Teleopti.Ccc.Web.Areas.Tennant
{
	public class TennantController : Controller
	{
		private readonly IApplicationAuthentication _applicationAuthentication;
		private readonly IIdentityAuthentication _identityAuthentication;
		
		
		public TennantController(IApplicationAuthentication applicationAuthentication, IIdentityAuthentication identityAuthentication)
		{
			_applicationAuthentication = applicationAuthentication;
			_identityAuthentication = identityAuthentication;
		}

		[HttpGet]
		[TennantUnitOfWork]
		public virtual JsonResult ApplicationLogon(string userName, string password)
		{
			var res = _applicationAuthentication.Logon(userName, password);
			return Json(res, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[TennantUnitOfWork]
		public virtual JsonResult IdentityLogon(string identity)
		{
			var res = _identityAuthentication.Logon(identity);
			return Json(res, JsonRequestBehavior.AllowGet);
		}
	}
}