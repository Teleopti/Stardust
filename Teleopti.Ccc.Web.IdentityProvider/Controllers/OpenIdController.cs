using System.Linq;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Web.IdentityProvider.Code;

namespace Teleopti.Ccc.Web.IdentityProvider.Controllers
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

					 return Util.ProcessAuthenticationChallenge(idrequest);
				}

				return View();
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
