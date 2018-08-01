using System;
using System.IdentityModel.Services;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Auth;
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
		private readonly ISessionSpecificWfmCookieProvider _sessionSpecificWfmCookieProvider;
		private readonly IAuthenticationModule _authenticationModule;
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly ICheckTenantUserExists _checkTenantUserExists;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonPersister _personPersister;

		public AuthenticationController(ILayoutBaseViewModelFactory layoutBaseViewModelFactory,
			IFormsAuthentication formsAuthentication, ISessionSpecificWfmCookieProvider sessionSpecificWfmCookieProvider,
			IAuthenticationModule authenticationModule, ICurrentHttpContext currentHttpContext,
			ICheckTenantUserExists checkTenantUserExists, ILoggedOnUser loggedOnUser, IPersonPersister  personPersister)
		{
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_formsAuthentication = formsAuthentication;
			_sessionSpecificWfmCookieProvider = sessionSpecificWfmCookieProvider;
			_authenticationModule = authenticationModule;
			_currentHttpContext = currentHttpContext;
			_checkTenantUserExists = checkTenantUserExists;
			_loggedOnUser = loggedOnUser;
			_personPersister = personPersister;

		}

		public ActionResult Index()
		{
			if (!_checkTenantUserExists.Exists())
				return Redirect("MultiTenancy/TenantAdminInfo");
			
			return RedirectToAction("", "Authentication");
		}

		public ViewResult SignIn()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel();
				return View();
		}

		[UnitOfWork]
		public virtual ActionResult SignOut()
		{
			_sessionSpecificWfmCookieProvider.RemoveCookie();
			_formsAuthentication.SignOut();

			_personPersister.InvalidateCachedCulure(_loggedOnUser.CurrentUser());

			var url = Request.UrlConsideringLoadBalancerHeaders();
			var issuerUrl = _authenticationModule.Issuer(_currentHttpContext.Current());

			var signInReply = new SignInRequestMessage(issuerUrl, _authenticationModule.Realm)
			{
				Context = "ru=" + url.AbsolutePath.Remove(url.AbsolutePath.IndexOf("Authentication/SignOut", StringComparison.OrdinalIgnoreCase)),
			};

			var signOut = new SignOutRequestMessage(issuerUrl, signInReply.WriteQueryString());

			return new RedirectResult(signOut.WriteQueryString());  
		}
	}

}