using System.Configuration;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class ReturnController : Controller
	{
		public ViewResult Hash()
		{
			return View(new ReturnModel { UseRelative = ConfigurationManager.AppSettings.ReadValue("UseRelativeConfiguration") });
		}

		[ValidateInput(false)]
		public ActionResult HandleReturn()
		{
			WSFederationMessage wsFederationMessage = WSFederationMessage.CreateFromNameValueCollection(WSFederationMessage.GetBaseUrl(ControllerContext.HttpContext.Request.Url), ControllerContext.HttpContext.Request.Form);
			if (wsFederationMessage.Context != null)
			{
				var wctx = HttpUtility.ParseQueryString(wsFederationMessage.Context);
				string returnUrl = wctx["ru"];
				if (!returnUrl.EndsWith("/"))
					returnUrl += "/";

				return new RedirectResult(returnUrl);
			}
			return new EmptyResult();
		}
	}

	public class ReturnModel
	{
		public bool UseRelative { get; set; }
	}
}