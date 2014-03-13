using System;
using System.Linq;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	public class OpenIdController : Controller
	{
		private readonly IOpenIdProviderWapper _openIdProvider;
		private readonly IWindowsAccountProvider _windowsAccountProvider;
		private readonly ICurrentHttpContext _currentHttpContext;

		public OpenIdController(IOpenIdProviderWapper openIdProvider, IWindowsAccountProvider windowsAccountProvider, ICurrentHttpContext currentHttpContext)
		{
			_openIdProvider = openIdProvider;
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
		public ActionResult Provider()
		{
			var request = _openIdProvider.GetRequest();

			// handles request from site
			if (request.IsResponseReady)
			{
				return _openIdProvider.PrepareResponse(request).AsActionResult();
			}

			// handles request from browser
			var idrequest = request as IAuthenticationRequest;

			var windowsAccount = _windowsAccountProvider.RetrieveWindowsAccount();
			if (windowsAccount != null)
			{
				var currentHttp = _currentHttpContext.Current();
				idrequest.LocalIdentifier =
					new Uri(currentHttp.Request.Url,
							currentHttp.Response.ApplyAppPathModifier("~/SSO/OpenId/AskUser/" + windowsAccount.UserName + "@" + windowsAccount.DomainName));
				idrequest.IsAuthenticated = true;
				_openIdProvider.SendResponse(request);
			}
			else
			{
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