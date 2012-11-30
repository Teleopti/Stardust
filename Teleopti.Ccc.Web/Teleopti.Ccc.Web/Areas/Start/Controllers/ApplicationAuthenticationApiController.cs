using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class ApplicationAuthenticationApiController : Controller
	{
		[HttpGet]
		public JsonResult CheckPassword(IAuthenticationModel model)
		{
			var result = model.AuthenticateUser();
			if (!result.Successful)
			{
				Response.StatusCode = 400;
				Response.TrySkipIisCustomErrors = true;
				ModelState.AddModelError("Error", result.Message);
				if (result.PasswordExpired)
					Response.SubStatusCode = 1;
				return ModelState.ToJson();
			}
			return Json(null);
		}
	}
}