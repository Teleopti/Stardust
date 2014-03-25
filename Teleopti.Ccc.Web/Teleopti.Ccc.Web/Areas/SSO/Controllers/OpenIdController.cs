using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Core.RequestContext;
using log4net;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	public class OpenIdController : Controller
	{
		private readonly IOpenIdProviderWapper _openIdProvider;
		private readonly IWindowsAccountProvider _windowsAccountProvider;
		private readonly ICurrentHttpContext _currentHttpContext;
		private static ILog _logger = LogManager.GetLogger(typeof(OpenIdController));

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
			_logger.Warn("Start of the OpenIdController.Provider()");
			var request = _openIdProvider.GetRequest();

			// handles request from site
			if (request.IsResponseReady)
			{
				return _openIdProvider.PrepareResponse(request).AsActionResult();
			}

			// handles request from browser
			var idrequest = request as IHostProcessedRequest;

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
			//throw new NotImplementedException();
			return null;
		}
	}
}