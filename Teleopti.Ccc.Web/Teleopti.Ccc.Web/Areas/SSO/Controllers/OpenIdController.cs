using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider.Behaviors;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Core.RequestContext;
using log4net;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	public class OpenIdController : Controller
	{
		private readonly IOpenIdProviderWapper _openIdProvider;
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly IProviderEndpointWrapper _providerEndpointWrapper;
		private static ILog _logger = LogManager.GetLogger(typeof(OpenIdController));

		public OpenIdController(IOpenIdProviderWapper openIdProvider, ICurrentHttpContext currentHttpContext, IProviderEndpointWrapper providerEndpointWrapper)
		{
			_openIdProvider = openIdProvider;
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

			if (!User.Identity.IsAuthenticated)
			{
				return this.RedirectToAction("SignIn", "Authentication", new { returnUrl = this.Url.Action("ProcessAuthRequest") });
			}

			return null;

		}

		public ActionResult AskUser()
		{
			return View();
		}

		public ActionResult ProcessAuthRequest()
		{
			if (_providerEndpointWrapper.PendingRequest == null)
			{
				return new ContentResult { Content = "Sorry, no PendingRequest" };
			}

			// Try responding immediately if possible.
			ActionResult response;
			if (this.AutoRespondIfPossible(out response))
			{
				return response;
			}

			// We can't respond immediately with a positive result.  But if we still have to respond immediately...
			if (_providerEndpointWrapper.PendingRequest.Immediate)
			{
				// We can't stop to prompt the user -- we must just return a negative response.
				return this.SendAssertion();
			}

			return null;
		}

		public ActionResult SendAssertion()
		{
			var pendingRequest = _providerEndpointWrapper.PendingRequest;
			var authReq = pendingRequest as IAuthenticationRequest;
			var anonReq = pendingRequest as IAnonymousRequest;
			_providerEndpointWrapper.PendingRequest = null; // clear session static so we don't do this again
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
				if (authReq.IsDirectedIdentity)
				{
					authReq.LocalIdentifier = createUserLocalIdentifier();
				}

				if (!authReq.IsDelegatedIdentifier)
				{
					authReq.ClaimedIdentifier = authReq.LocalIdentifier;
				}
			}

			return _openIdProvider.PrepareResponse(pendingRequest).AsActionResult();
		}


		private bool AutoRespondIfPossible(out ActionResult response)
		{
			// If the odds are good we can respond to this one immediately (without prompting the user)...
			if (_providerEndpointWrapper.PendingRequest.IsReturnUrlDiscoverable(_openIdProvider.Channel().WebRequestHandler) == RelyingPartyDiscoveryResult.Success
				&& User.Identity.IsAuthenticated)
			{
				// Is this is an identity authentication request? (as opposed to an anonymous request)...
				if (_providerEndpointWrapper.PendingAuthenticationRequest != null)
				{
					// If this is directed identity, or if the claimed identifier being checked is controlled by the current user...
					if (_providerEndpointWrapper.PendingAuthenticationRequest.IsDirectedIdentity
						|| UserControlsIdentifier(_providerEndpointWrapper.PendingAuthenticationRequest))
					{
						_providerEndpointWrapper.PendingAuthenticationRequest.IsAuthenticated = true;
						response = this.SendAssertion();
						return true;
					}
				}

				// If this is an anonymous request, we can respond to that too.
				if (_providerEndpointWrapper.PendingAnonymousRequest != null)
				{
					_providerEndpointWrapper.PendingAnonymousRequest.IsApproved = true;
					response = this.SendAssertion();
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
				throw new ArgumentNullException("authReq");
			}

			if (User == null || User.Identity == null)
			{
				return false;
			}

			var userLocalIdentifier = createUserLocalIdentifier();

			// Assuming the URLs on the web server are not case sensitive (on Windows servers they almost never are),
			// and usernames aren't either, compare the identifiers without case sensitivity.
			// No reason to do this for the PPID identifiers though, since they *can* be case sensitive and are highly
			// unlikely to be typed in by the user anyway.
			return string.Equals(authReq.LocalIdentifier.ToString(), userLocalIdentifier.ToString(), StringComparison.OrdinalIgnoreCase) ||
				authReq.LocalIdentifier == PpidGeneration.PpidIdentifierProvider.GetIdentifier(userLocalIdentifier, authReq.Realm);
		}

		private Uri createUserLocalIdentifier()
		{
			var currentHttp = _currentHttpContext.Current();
			Uri userLocalIdentifier = new Uri(currentHttp.Request.Url,
				currentHttp.Response.ApplyAppPathModifier("~/OpenId/AskUser/" + User.Identity.Name + "@datasource"));
			return userLocalIdentifier;
		}
	}
}