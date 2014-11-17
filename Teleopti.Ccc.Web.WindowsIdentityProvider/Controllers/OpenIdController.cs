using System;
using System.Configuration;
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
		private static readonly ILog _logger = LogManager.GetLogger(typeof(OpenIdController));

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
			var useLocalhostIdentifierSetting = ConfigurationManager.AppSettings["UseLocalhostIdentifier"];
			var useLocalhostIdentifier = !string.IsNullOrEmpty(useLocalhostIdentifierSetting) && bool.Parse(useLocalhostIdentifierSetting);
				var idrequest = _providerEndpointWrapper.PendingRequest as IAuthenticationRequest;
			_providerEndpointWrapper.PendingRequest = null;
			if (useLocalhostIdentifier && idrequest.ProviderEndpoint != null)
			{
				idrequest.ProviderEndpoint = new Uri(new Uri(ConfigurationManager.AppSettings["CustomEndpointHost"] ?? "http://localhost/"), idrequest.ProviderEndpoint.MakeRelativeUri(
					new Uri(idrequest.ProviderEndpoint.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped))));
			} 
			if (idrequest.IsReturnUrlDiscoverable(_openIdProvider.WebRequestHandler()) != RelyingPartyDiscoveryResult.Success)
			{
				idrequest.IsAuthenticated = false;
				return new EmptyResult();
			}
			var windowsAccount = _windowsAccountProvider.RetrieveWindowsAccount();
			if (windowsAccount != null)
			{
				_logger.Warn("Found WindowsAccount");
				var currentHttp = _currentHttpContext.Current();
				var userIdentifier =
					currentHttp.Response.ApplyAppPathModifier("~/OpenId/AskUser/" +
					                                          Uri.EscapeDataString(windowsAccount.DomainName + "#" +
					                                                               windowsAccount.UserName.Replace(".", "$$$")));
				var identifier = new Uri(currentHttp.Request.Url,userIdentifier);
				if (useLocalhostIdentifier)
				{
					identifier = new Uri(new Uri(ConfigurationManager.AppSettings["CustomEndpointIdentifier"] ?? "http://localhost/"), userIdentifier);
				}
				idrequest.LocalIdentifier = identifier;
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