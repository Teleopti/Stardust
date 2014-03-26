using System;
using System.Linq;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider;
using log4net;
using Teleopti.Ccc.Web.WindowsIdentityProvider.Core;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Controllers
{
	public class OpenIdController : Controller
	{
		private readonly IOpenIdProviderWrapper _openIdProvider;
		private readonly IWindowsAccountProvider _windowsAccountProvider;
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly IProviderEndpointWrapper _providerEndpointWrapper;
		private static ILog _logger = LogManager.GetLogger(typeof(OpenIdController));

		public OpenIdController()
			: this(
				new OpenIdProviderWrapper(new OpenIdProvider(OpenIdProvider.HttpApplicationStore)),
				new WindowsAccountProvider(new CurrentHttpContext()), new CurrentHttpContext(), new ProviderEndpointWrapper())
		{
		}

		public OpenIdController(IOpenIdProviderWrapper openIdProviderWrapper, IWindowsAccountProvider windowsAccountProvider, ICurrentHttpContext currentHttpContext, IProviderEndpointWrapper providerEndpointWrapper)
		{
			_openIdProvider = openIdProviderWrapper;
			_windowsAccountProvider = windowsAccountProvider;
			_currentHttpContext = currentHttpContext;
			_providerEndpointWrapper = providerEndpointWrapper;
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
			_providerEndpointWrapper.PendingRequest = (IHostProcessedRequest)request;
			return RedirectToAction("TriggerWindowsAuthorization");
		}

		[Authorize]
		public ActionResult TriggerWindowsAuthorization()
		{
			var idrequest = _providerEndpointWrapper.PendingRequest as IAuthenticationRequest;
			_providerEndpointWrapper.PendingRequest = null;

			var windowsAccount = _windowsAccountProvider.RetrieveWindowsAccount();
			if (windowsAccount != null)
			{
				_logger.Warn("Found WindowsAccount");
				var currentHttp = _currentHttpContext.Current();
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