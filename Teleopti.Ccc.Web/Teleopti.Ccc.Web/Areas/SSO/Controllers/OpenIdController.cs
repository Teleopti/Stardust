using System;
using System.Linq;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Provider.Behaviors;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext;
using log4net;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	public class OpenIdController : Controller
	{
		private readonly IOpenIdProviderWapper _openIdProvider;
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IFormsAuthentication _formsAuthentication;
		private static ILog _logger = LogManager.GetLogger(typeof(OpenIdController));

		public OpenIdController(IOpenIdProviderWapper openIdProvider, ICurrentHttpContext currentHttpContext,ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IFormsAuthentication formsAuthentication)
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
		public ActionResult Provider()
		{
			_logger.Warn("Start of the OpenIdController.Provider()");
			var request = _openIdProvider.GetRequest();
			if (request != null)
			{
				// handles request from site
				if (request.IsResponseReady)
				{
					return _openIdProvider.PrepareResponse(request).AsActionResult();
				}

				var pendingRequest = Convert.ToBase64String(SerializationHelper.SerializeAsBinary(request).ToCompressedByteArray());
					
				// handles request from browser
				string userName;
				if (!_formsAuthentication.TryGetCurrentUser(out userName))
				{
					ViewBag.ReturnUrl = Url.Action("ProcessAuthRequest");
					ViewBag.PendingRequest = pendingRequest;
					return SignIn();
				}

				return ProcessAuthRequest(pendingRequest);
			}
			return new EmptyResult();

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
		public ActionResult ProcessAuthRequest(string pendingRequest)
		{
			var request = getPendingRequest(pendingRequest);
			if (request == null)
			{
				return new ContentResult { Content = "Sorry, no PendingRequest" };
			}

			// Try responding immediately if possible.
			ActionResult response;
			if (AutoRespondIfPossible(request, out response))
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

		private ActionResult sendAssertion(IHostProcessedRequest pendingRequest)
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
			}

			return _openIdProvider.PrepareResponse(pendingRequest).AsActionResult();
		}


		private bool AutoRespondIfPossible(IHostProcessedRequest pendingRequest, out ActionResult response)
		{
			string userName;
			if (_formsAuthentication.TryGetCurrentUser(out userName))
			{
				var pending = pendingRequest as IAuthenticationRequest;
				// Is this is an identity authentication request? (as opposed to an anonymous request)...
				if (pending != null)
				{
					// If this is directed identity, or if the claimed identifier being checked is controlled by the current user...
					if (pending.IsDirectedIdentity
						|| UserControlsIdentifier(pending))
					{
						pending.IsAuthenticated = true;
						response = sendAssertion(pendingRequest);
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
				throw new ArgumentNullException("authReq");
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
			var currentHttp = _currentHttpContext.Current();
			Uri userLocalIdentifier = new Uri(currentHttp.Request.Url,
				currentHttp.Response.ApplyAppPathModifier("~/SSO/OpenId/AskUser/" + userName));
			return userLocalIdentifier;
		}
	}
}