using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Provider;
using log4net;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.Web.WindowsIdentityProvider.Core;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Controllers
{
	public class OpenIdController : Controller
	{
		private readonly IOpenIdProviderWrapper _openIdProvider;
		private readonly IWindowsAccountProvider _windowsAccountProvider;
		private readonly ICurrentHttpContext _currentHttpContext;
		private static readonly ILog logger = LogManager.GetLogger(typeof(OpenIdController));

		public OpenIdController(IOpenIdProviderWrapper openIdProviderWrapper, IWindowsAccountProvider windowsAccountProvider, ICurrentHttpContext currentHttpContext)
		{
			_openIdProvider = openIdProviderWrapper;
			_windowsAccountProvider = windowsAccountProvider;
			_currentHttpContext = currentHttpContext;
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
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual ActionResult Provider()
		{
			logger.Info("Start of the OpenIdController.Provider()");
			if (isHeadRequest())
			{
				logger.Debug("Found HEAD-request");
				return new EmptyResult();
			}
			var request = _openIdProvider.GetRequest();

			// handles request from site
			if (request.IsResponseReady)
			{
				return _openIdProvider.PrepareResponse(request).AsActionResult();
			}

			// handles request from browser
			var pendingRequest = Convert.ToBase64String(SerializationHelper.SerializeAsBinary(request).ToCompressedByteArray());
			ViewBag.ReturnUrl = Url.Action("TriggerWindowsAuthorization");
			ViewBag.PendingRequest = pendingRequest;
			return View("Redirect");
		}

		private bool isHeadRequest()
		{
			return _currentHttpContext.Current().Request.HttpMethod.Equals("HEAD", StringComparison.InvariantCultureIgnoreCase);
		}

		private IHostProcessedRequest getPendingRequest(string request)
		{
			var pendingRequest = SerializationHelper.Deserialize<IHostProcessedRequest>(Convert.FromBase64String(request).ToUncompressedString());

			return pendingRequest;
		}

		[Authorize]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual ActionResult TriggerWindowsAuthorization(string pendingRequest)
		{
			logger.Info("Start of the OpenIdController.TriggerWindowsAuthorization()");

			var useLocalhostIdentifierSetting = ConfigurationManager.AppSettings["UseLocalhostIdentifier"];
			var useLocalhostIdentifier = !string.IsNullOrEmpty(useLocalhostIdentifierSetting) && bool.Parse(useLocalhostIdentifierSetting);
			if (useLocalhostIdentifier)
			{
				return makeWindowsAuthenticationUsingLocalhostIdentifier(getPendingRequest(pendingRequest));
			}

			return makeWindowsAuthentication(getPendingRequest(pendingRequest));
		}

		private ActionResult makeWindowsAuthentication(IHostProcessedRequest pendingRequest)
		{
			var idrequest = pendingRequest as IAuthenticationRequest;

			var relyingPartyDiscoveryResult = idrequest.IsReturnUrlDiscoverable(_openIdProvider.WebRequestHandler());
			if (relyingPartyDiscoveryResult != RelyingPartyDiscoveryResult.Success)
			{
				idrequest.IsAuthenticated = false;
				logger.WarnFormat("The return url discovery failed with result {0}", relyingPartyDiscoveryResult);
				return new EmptyResult();
			}

			var windowsAccount = _windowsAccountProvider.RetrieveWindowsAccount();
			if (windowsAccount != null)
			{
				logger.Warn("Found WindowsAccount");
				var currentHttp = _currentHttpContext.Current();
				var userIdentifier =
					currentHttp.Response.ApplyAppPathModifier("~/OpenId/AskUser/" +
					                                          Uri.EscapeDataString(windowsAccount.DomainName + "#" +
					                                                               windowsAccount.UserName.Replace(".", "$$$")));
				var identifier = new Uri(currentHttp.Request.Url, userIdentifier);
				idrequest.LocalIdentifier = identifier;
				idrequest.IsAuthenticated = true;
				var fetchResponse = new FetchResponse();
				fetchResponse.Attributes.Add(new AttributeValues(ClaimTypes.IsPersistent, "true"));
				idrequest.AddResponseExtension(fetchResponse);
				_openIdProvider.SendResponse(idrequest);
			}
			else
			{
				logger.Warn("NOT Found WindowsAccount");
				idrequest.IsAuthenticated = false;
			}
			return new EmptyResult();
		}

		private ActionResult makeWindowsAuthenticationUsingLocalhostIdentifier(IHostProcessedRequest pendingRequest)
		{
			var idrequest = pendingRequest as IAuthenticationRequest;

			var requestMessageProperty = idrequest.GetType()
				.GetProperty("RequestMessage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			var requestMessage = requestMessageProperty.GetValue(idrequest, null);

			var returnToProperty = requestMessage.GetType()
				.GetProperty("ReturnTo", BindingFlags.Instance | BindingFlags.NonPublic);
			var returnToUri = (Uri) returnToProperty.GetValue(requestMessage, null);

			if (idrequest.ProviderEndpoint != null)
			{
				idrequest.ProviderEndpoint =
					new Uri(new Uri(ConfigurationManager.AppSettings["CustomEndpointHost"] ?? "http://localhost/"),
						new Uri(idrequest.ProviderEndpoint.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped))
							.MakeRelativeUri(idrequest.ProviderEndpoint));
				var realmUriField = typeof (Realm).GetField("uri", BindingFlags.NonPublic | BindingFlags.Instance);
				var realmUri = (Uri) realmUriField.GetValue(idrequest.Realm);

				realmUri = new Uri(new Uri(ConfigurationManager.AppSettings["CustomEndpointHost"] ?? "http://localhost/"),
					new Uri(realmUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped))
						.MakeRelativeUri(realmUri));
				realmUriField.SetValue(idrequest.Realm, realmUri);

				var temporaryReturnToUri =
					new Uri(new Uri(ConfigurationManager.AppSettings["CustomEndpointHost"] ?? "http://localhost/"),
						new Uri(returnToUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped))
							.MakeRelativeUri(returnToUri));
				returnToProperty.SetValue(requestMessage, temporaryReturnToUri, null);
			}
			var relyingPartyDiscoveryResult = idrequest.IsReturnUrlDiscoverable(_openIdProvider.WebRequestHandler());
			if (relyingPartyDiscoveryResult != RelyingPartyDiscoveryResult.Success)
			{
				idrequest.IsAuthenticated = false;
				logger.WarnFormat("The return url discovery failed with result {0}", relyingPartyDiscoveryResult);
				return new EmptyResult();
			}
			returnToProperty.SetValue(requestMessage, returnToUri, null);
			var windowsAccount = _windowsAccountProvider.RetrieveWindowsAccount();
			if (windowsAccount != null)
			{
				logger.Warn("Found WindowsAccount");
				var currentHttp = _currentHttpContext.Current();
				var userIdentifier =
					currentHttp.Response.ApplyAppPathModifier("~/OpenId/AskUser/" +
					                                          Uri.EscapeDataString(windowsAccount.DomainName + "#" +
					                                                               windowsAccount.UserName.Replace(".", "$$$")));
				var identifier = new Uri(currentHttp.Request.Url, userIdentifier);
				identifier = new Uri(new Uri(ConfigurationManager.AppSettings["CustomEndpointHost"] ?? "http://localhost/"),
					new Uri(identifier.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(identifier));
				idrequest.LocalIdentifier = identifier;
				idrequest.IsAuthenticated = true;
				_openIdProvider.SendResponse(idrequest);
			}
			else
			{
				logger.Warn("NOT Found WindowsAccount");
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