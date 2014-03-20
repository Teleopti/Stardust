using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider;
using log4net;
using Teleopti.Ccc.Web.WindowsIdentityProvider.Core;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Controllers
{
	public class OpenIdController : Controller
	{
		private readonly IOpenIdProviderWapper _openIdProvider;
		private readonly IWindowsAccountProvider _windowsAccountProvider;
		private static ILog _logger = LogManager.GetLogger(typeof(OpenIdController));

		public OpenIdController()
		{
			_openIdProvider = new OpenIdProviderWapper(new OpenIdProvider(OpenIdProvider.HttpApplicationStore));
			_windowsAccountProvider = new WindowsAccountProvider();
		}

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
			_logger.Warn("Start of the OpenIdController.Provider()");
			var request = _openIdProvider.GetRequest();

			// handles request from site
			if (request.IsResponseReady)
			{
				return _openIdProvider.PrepareResponse(request).AsActionResult();
			}

			// handles request from browser
			ProviderEndpoint.PendingRequest = (IHostProcessedRequest) request;
			return RedirectToAction("TriggerWindowsAuthorization");
		}

		[Authorize]
		public ActionResult TriggerWindowsAuthorization()
		{
			var idrequest = ProviderEndpoint.PendingRequest as IAuthenticationRequest;
			ProviderEndpoint.PendingRequest = null;

			var windowsAccount = _windowsAccountProvider.RetrieveWindowsAccount();
			if (windowsAccount != null)
			{
				_logger.Warn("Found WindowsAccount");
				var currentHttp = HttpContext;
				idrequest.LocalIdentifier =
					new Uri(currentHttp.Request.Url,
							currentHttp.Response.ApplyAppPathModifier("~/OpenId/AskUser/" + windowsAccount.UserName + "@" + windowsAccount.DomainName));
				idrequest.IsAuthenticated = true;
				_openIdProvider.SendResponse(idrequest);
			}
			else
			{
				_logger.Warn("NOT Found WindowsAccount");
				idrequest.IsAuthenticated = false;
			}
			return new EmptyResult();
		}

		public ActionResult AskUser()
		{
			return View();
		}
	}
	
}