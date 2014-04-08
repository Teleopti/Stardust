using System;
using System.Web.Mvc;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationController : Controller
	{
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IFormsAuthentication _formsAuthentication;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IAuthenticationModule _authenticationModule;

		public AuthenticationController(ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IFormsAuthentication formsAuthentication, ISessionSpecificDataProvider sessionSpecificDataProvider, IAuthenticationModule authenticationModule)
		{
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_formsAuthentication = formsAuthentication;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_authenticationModule = authenticationModule;
		}

		public ActionResult Index()
		{
			return RedirectToAction("", "Authentication");
		}

		public ViewResult SignIn()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel();
			return View();
		}

		public ActionResult SignOut()
		{
			_formsAuthentication.SignOut();
			return RedirectToAction("", "Authentication");
		}

		public ActionResult SignInAsAnotherUser()
		{
			_sessionSpecificDataProvider.RemoveCookie();
			_formsAuthentication.SignOut();

			var url = Request.Url;
			var signIn = new SignInRequestMessage(new Uri(_authenticationModule.Issuer), _authenticationModule.Realm)
			{
				Context = "ru=" + Request.Path,
				Reply = url.Scheme + Uri.SchemeDelimiter + url.Host + (url.IsDefaultPort ? "" : ":" + url.Port)
			};

			return new RedirectResult(signIn.WriteQueryString());
		}
	}

}