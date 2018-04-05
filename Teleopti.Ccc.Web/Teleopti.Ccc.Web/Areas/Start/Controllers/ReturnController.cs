using System;
using System.Configuration;
using System.IdentityModel.Services;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Web.Auth;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class ReturnController : Controller
	{
		public ViewResult Hash()
		{
			return View(
				new ReturnModel
				{
					UseRelative = ConfigurationManager.AppSettings.ReadValue("UseRelativeConfiguration"),
					ApplicationArea = WebApplicationAreaResolver.GetWebApplicationArea(Request, Url.Content("~/"))
				});
		}
		
		[ValidateInput(false)]
		public ActionResult HandleReturn()
		{
			var returnUrl = getWSFederationMessageReturnUrl();
			if (!String.IsNullOrEmpty(returnUrl))
			{
				return new RedirectResult(returnUrl);
			}
			return new RedirectResult("~/Mytime");
		}

		private string getWSFederationMessageReturnUrl()
		{
			var wsFederationMessage =
				WSFederationMessage.CreateFromNameValueCollection(
					WSFederationMessage.GetBaseUrl(ControllerContext.HttpContext.Request.UrlConsideringLoadBalancerHeaders()),
					ControllerContext.HttpContext.Request.Form);
			if (wsFederationMessage == null || wsFederationMessage.Context == null) return null;

			return getReturnUrlQueryParameterFromUrl(wsFederationMessage.Context);

		}

		private string getReturnUrlQueryParameterFromUrl(string context)
		{
			var queryNameValueCollection = HttpUtility.ParseQueryString(context);

			var returnUrl = queryNameValueCollection["ru"];
			if (!String.IsNullOrEmpty (returnUrl))
			{
				if (!returnUrl.EndsWith ("/"))
					returnUrl += "/";
			}
			return returnUrl;
		}
		
	}

	public class ReturnModel
	{
		public bool UseRelative { get; set; }
		public string ApplicationArea { get; set; }
	}
}