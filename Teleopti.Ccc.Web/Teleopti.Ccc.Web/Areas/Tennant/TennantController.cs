using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;
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
			// for now we don't use the statuscode
			//if (!res.Success)
			//{
			//	Response.TrySkipIisCustomErrors = true;
			//	Response.StatusCode = 401;
			//}

			return Json(res, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult IdentityLogon(string identity)
		{
			var res = _identityAuthentication.Logon(identity);
			// for now we don't use the statuscode
			//if (!res.Success)
			//{
			//	Response.TrySkipIisCustomErrors = true;
			//	Response.StatusCode = 401;
			//}
		
			return Json(res, JsonRequestBehavior.AllowGet);
		}
	}
}