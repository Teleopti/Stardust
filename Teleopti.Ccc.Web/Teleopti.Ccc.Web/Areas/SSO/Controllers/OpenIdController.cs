using System;
using System.Linq;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.Messaging;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	 public class OpenIdController : Controller
	 {
		 internal static OpenIdProvider OpenIdProvider = new OpenIdProvider();

		 public ActionResult Identifier()
		  {
				if (Request.AcceptTypes != null && Request.AcceptTypes.Contains("application/xrds+xml"))
				{
					 return View("Xrds");
				}
				return View();
		  }

		  [ValidateInput(false)]
		  public ActionResult Provider()
		  {
				var request = OpenIdProvider.GetRequest();

				if (request != null)
				{
					 if (request.IsResponseReady)
					 {
						  return OpenIdProvider.PrepareResponse(request).AsActionResult();
					 }

					 var idrequest = request as IAuthenticationRequest;

					 idrequest.LocalIdentifier = buildIdentityUrl();
					 idrequest.IsAuthenticated = true;
					 ProviderEndpoint.Provider.SendResponse(request);
				}

				return new EmptyResult();
		  }

		  private static Identifier buildIdentityUrl()
		  {
			  var username = System.Web.HttpContext.Current.User.Identity.Name;
			  var slash = username.IndexOf('\\');
			  if (slash >= 0)
			  {
				  username = username.Substring(slash + 1);
			  }
			  return new Uri(System.Web.HttpContext.Current.Request.Url, System.Web.HttpContext.Current.Response.ApplyAppPathModifier("~/SSO/OpenId/AskUser/" + username));
		  }

		  public ActionResult AskUser()
		  {
				return View();
		  }

		  public ActionResult Xrds()
		  {
				return View();
		  }
	 }
}
