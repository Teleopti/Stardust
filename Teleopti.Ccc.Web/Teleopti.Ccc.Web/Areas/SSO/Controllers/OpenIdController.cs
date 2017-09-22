using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider.Behaviors;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Core;
using log4net;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	public class OpenIdController : Controller
	{
		private readonly IOpenIdProviderWapper _openIdProvider;
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IFormsAuthentication _formsAuthentication;
		private static readonly ILog logger = LogManager.GetLogger(typeof(OpenIdController));

		public OpenIdController(IOpenIdProviderWapper openIdProvider, ICurrentHttpContext currentHttpContext,ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IFormsAuthentication formsAuthentication, IToggleManager toggleManager)
		{
			_openIdProvider = openIdProvider;
			_currentHttpContext = currentHttpContext;
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_formsAuthentication = formsAuthentication;
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
			logger.Debug("Start of the OpenIdController.Provider()");
			if (isHeadRequest())
			{
				logger.Debug("Found HEAD-request");
				return new EmptyResult();
			}
			var request = _openIdProvider.GetRequest();
			if (request != null)
			{
				// handles request from site
				if (request.IsResponseReady)
				{
					return _openIdProvider.PrepareResponse(request).AsActionResultMvc5();
				}
				// handles request from browser
				var pendingRequest = Convert.ToBase64String(SerializationHelper.SerializeAsBinary(request).ToCompressedByteArray());
				string userName;
				if (!_formsAuthentication.TryGetCurrentUser(out userName))
				{
					ViewBag.ReturnUrl = Url.Action("ProcessAuthRequest");
					ViewBag.PendingRequest = pendingRequest;
					return SignIn();
				}

				return ProcessAuthRequest(pendingRequest, false);
			}
			return new EmptyResult();

		}

		private bool isHeadRequest()
		{
			return Request.HttpMethod.Equals(HttpMethod.Head.Method, StringComparison.InvariantCultureIgnoreCase);
		}

		public ViewResult SignIn()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel();
	
			return View("SignIn");
		}

		public ActionResult AskUser()
		{
			return View();
		}

		[HttpPost]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual ActionResult ProcessAuthRequest(string pendingRequest, bool isPersistent)
		{
			var request = getPendingRequest(pendingRequest);
			if (request == null)
			{
				return new ContentResult { Content = "Sorry, no PendingRequest" };
			}

			// Try responding immediately if possible.
			ActionResult response;
			if (AutoRespondIfPossible(request, isPersistent, out response))
			{
				return response;
			}

			// We can't respond immediately with a positive result.  But if we still have to respond immediately...
			if (request.Immediate)
			{
				// We can't stop to prompt the user -- we must just return a negative response.
				return sendAssertion(request);
			}

			return null;
		}

		private IHostProcessedRequest getPendingRequest(string request)
		{
			var pendingRequest = SerializationHelper.Deserialize<IHostProcessedRequest>(Convert.FromBase64String(request).ToUncompressedString());

			return pendingRequest;
		}

		private ActionResult sendAssertion(IHostProcessedRequest pendingRequest, bool isPersistent = false)
		{
			var authReq = pendingRequest as IAuthenticationRequest;
			var anonReq = pendingRequest as IAnonymousRequest;
			if (pendingRequest == null)
			{
				throw new InvalidOperationException("There's no pending authentication request!");
			}

			// Set safe defaults if somehow the user ended up (perhaps through XSRF) here before electing to send data to the RP.
			if (anonReq != null && !anonReq.IsApproved.HasValue)
			{
				anonReq.IsApproved = false;
			}

			if (authReq != null && !authReq.IsAuthenticated.HasValue)
			{
				authReq.IsAuthenticated = false;
			}

			if (authReq != null && authReq.IsAuthenticated.Value)
			{
				string userName;
				_formsAuthentication.TryGetCurrentUser(out userName);
				if (authReq.IsDirectedIdentity)
				{
					authReq.LocalIdentifier = createUserLocalIdentifier(userName);
				}

				if (!authReq.IsDelegatedIdentifier)
				{
					authReq.ClaimedIdentifier = authReq.LocalIdentifier;
				}
				var fetchResponse = new FetchResponse();
				fetchResponse.Attributes.Add(new AttributeValues(ClaimTypes.IsPersistent, isPersistent.ToString().ToLowerInvariant()));
				authReq.AddResponseExtension(fetchResponse);
			}

			return _openIdProvider.PrepareResponse(pendingRequest).AsActionResultMvc5();
		}


		private bool AutoRespondIfPossible(IHostProcessedRequest pendingRequest, bool isPersistent, out ActionResult response)
		{
			string userName;
			if (_formsAuthentication.TryGetCurrentUser(out userName))
			{
				var pending = pendingRequest as IAuthenticationRequest;
				// Is this is an identity authentication request? (as opposed to an anonymous request)...
				if (pending != null)
				{
					var useLocalhostIdentifier = ConfigurationManager.AppSettings.ReadValue("UseLocalhostIdentifier");
					Uri returnToUri = null;
					object requestMessage = null;
					PropertyInfo returnToProperty = null;
					if (useLocalhostIdentifier && pending.ProviderEndpoint != null)
					{
						pending.ProviderEndpoint =
							new Uri(new Uri(ConfigurationManager.AppSettings["CustomEndpointHost"] ?? "http://localhost/"),
								new Uri(pending.ProviderEndpoint.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped))
									.MakeRelativeUri(pending.ProviderEndpoint));

						var realmUriField = typeof(Realm).GetField("uri", BindingFlags.NonPublic | BindingFlags.Instance);
						var realmUri = (Uri)realmUriField.GetValue(pending.Realm);

						realmUri = new Uri(new Uri(ConfigurationManager.AppSettings["CustomEndpointHost"] ?? "http://localhost/"),
							new Uri(realmUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped))
								.MakeRelativeUri(realmUri));
						realmUriField.SetValue(pending.Realm, realmUri);

						var requestMessageProperty = pending.GetType()
							.GetProperty("RequestMessage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
						requestMessage = requestMessageProperty.GetValue(pending, null);

						returnToProperty = requestMessage.GetType()
							.GetProperty("ReturnTo", BindingFlags.Instance | BindingFlags.NonPublic);
						returnToUri = (Uri)returnToProperty.GetValue(requestMessage, null);

						var temporaryReturnToUri =
							new Uri(new Uri(ConfigurationManager.AppSettings["CustomEndpointHost"] ?? "http://localhost/"),
								new Uri(returnToUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped))
									.MakeRelativeUri(returnToUri));
						returnToProperty.SetValue(requestMessage, temporaryReturnToUri, null);
					}

					var relyingPartyDiscoveryResult = pendingRequest.IsReturnUrlDiscoverable(_openIdProvider.WebRequestHandler());
					if (relyingPartyDiscoveryResult != RelyingPartyDiscoveryResult.Success)
					{
						pending.IsAuthenticated = false;
						logger.WarnFormat("The return url discovery failed with result {0}", relyingPartyDiscoveryResult);
						response = null;
						return false;
					}
					if (useLocalhostIdentifier)
					{
						returnToProperty.SetValue(requestMessage, returnToUri, null);
					}
					// If this is directed identity, or if the claimed identifier being checked is controlled by the current user...
					if (pending.IsDirectedIdentity
						|| UserControlsIdentifier(pending))
					{
						pending.IsAuthenticated = true;
						response = sendAssertion(pendingRequest, isPersistent);
						return true;
					}
				}

				// If this is an anonymous request, we can respond to that too.
				var pendingAnonymousRequest = pendingRequest as IAnonymousRequest;
				if (pendingAnonymousRequest != null)
				{
					pendingAnonymousRequest.IsApproved = true;
					response = sendAssertion(pendingRequest);
					return true;
				}
			}

			response = null;
			return false;
		}

		private bool UserControlsIdentifier(IAuthenticationRequest authReq)
		{
			if (authReq == null)
			{
				throw new ArgumentNullException(nameof(authReq));
			}

			string userName;
			if (!_formsAuthentication.TryGetCurrentUser(out userName))
			{
				return false;
			}

			var userLocalIdentifier = createUserLocalIdentifier(userName);

			// Assuming the URLs on the web server are not case sensitive (on Windows servers they almost never are),
			// and usernames aren't either, compare the identifiers without case sensitivity.
			// No reason to do this for the PPID identifiers though, since they *can* be case sensitive and are highly
			// unlikely to be typed in by the user anyway.
			return string.Equals(authReq.LocalIdentifier.ToString(), userLocalIdentifier.ToString(), StringComparison.OrdinalIgnoreCase) ||
				authReq.LocalIdentifier == PpidGeneration.PpidIdentifierProvider.GetIdentifier(userLocalIdentifier, authReq.Realm);
		}

		private Uri createUserLocalIdentifier(string userName)
		{
			var useLocalhostIdentifier = ConfigurationManager.AppSettings.ReadValue("UseLocalhostIdentifier");
			var currentHttp = _currentHttpContext.Current();

			var userIdentifier = currentHttp.Response.ApplyAppPathModifier("~/SSO/OpenId/AskUser/" + userName);
			var identifier = new Uri(currentHttp.Request.Url, userIdentifier);
			if (useLocalhostIdentifier)
			{
				identifier = new Uri(new Uri(ConfigurationManager.AppSettings["CustomEndpointHost"] ?? "http://localhost/"),
					new Uri(identifier.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(identifier));
			}
			
			return identifier;
		}
	}
}