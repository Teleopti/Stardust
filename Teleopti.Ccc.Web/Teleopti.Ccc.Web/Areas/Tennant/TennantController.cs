using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Tennant.Core;

namespace Teleopti.Ccc.Web.Areas.Tennant
{
	public class TennantController : Controller
	{
		private readonly IApplicationAuthentication _applicationAuthentication;

		public TennantController(IApplicationAuthentication applicationAuthentication)
		{
			_applicationAuthentication = applicationAuthentication;
		}

		[HttpGet]
		public JsonResult ApplicationLogon(string userName, string password)
		{
			var res = _applicationAuthentication.Logon(userName, password);
			if (!res.Success)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 401;
			}

			return Json(res, JsonRequestBehavior.AllowGet);
		}
	}
}