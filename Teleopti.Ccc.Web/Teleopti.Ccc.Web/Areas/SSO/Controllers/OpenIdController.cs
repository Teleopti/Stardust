using System;
using System.Linq;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Web.Areas.SSO.Core;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	 public class OpenIdController : Controller
	 {
		  internal static OpenIdProvider OpenIdProvider = new OpenIdProvider();

		  public ActionResult Identifier()
		  {
				if (User.Identity.IsAuthenticated && ProviderEndpoint.PendingAuthenticationRequest != null)
				{
					 Util.ProcessAuthenticationChallenge(ProviderEndpoint.PendingAuthenticationRequest);
					 if (ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated.HasValue)
					 {
						  ProviderEndpoint.SendResponse();
					 }
				}

				if (Request.AcceptTypes != null && Request.AcceptTypes.Contains("application/xrds+xml"))
				{
					 return new TransferResult("~/OpenId/Xrds");
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

					 ProviderEndpoint.PendingRequest = (IHostProcessedRequest)request;
					 var idrequest = request as IAuthenticationRequest;

					 idrequest.LocalIdentifier = buildIdentityUrl();
					 idrequest.IsAuthenticated = true;
					 ProviderEndpoint.SendResponse();
				}

				return new EmptyResult();
		  }


		  private static Identifier buildIdentityUrl()
		  {
			  string username = System.Web.HttpContext.Current.User.Identity.Name;
			  int slash = username.IndexOf('\\');
			  if (slash >= 0)
			  {
				  username = username.Substring(slash + 1);
			  }
			  return buildIdentityUrl(username);
		  }

		  private static Identifier buildIdentityUrl(string username)
		  {
			  // This sample Provider has a custom policy for normalizing URIs, which is that the whole
			  // path of the URI be lowercase except for the first letter of the username.
			  username = username.Substring(0, 1).ToUpperInvariant() + username.Substring(1).ToLowerInvariant();
			  return new Uri(System.Web.HttpContext.Current.Request.Url, System.Web.HttpContext.Current.Response.ApplyAppPathModifier("~/OpenId/AskUser/" + username));
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
