﻿using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.Web;
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
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly ILoadAllTenantsUsers _loadAllTenantsUsers;

		public AuthenticationController(ILayoutBaseViewModelFactory layoutBaseViewModelFactory,
			IFormsAuthentication formsAuthentication, ISessionSpecificDataProvider sessionSpecificDataProvider,
			IAuthenticationModule authenticationModule, ICurrentHttpContext currentHttpContext,
			ILoadAllTenantsUsers loadAllTenantsUsers)
		{
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_formsAuthentication = formsAuthentication;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_authenticationModule = authenticationModule;
			_currentHttpContext = currentHttpContext;
			_loadAllTenantsUsers = loadAllTenantsUsers;
		}

		public ActionResult Index()
		{
			if (!_loadAllTenantsUsers.TenantUsers().Any())
				return Redirect("MultiTenancy/TenantAdminInfo");
			
			return RedirectToAction("", "Authentication");
		}

		public ViewResult SignIn()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel();
			return View();
		}

		public ActionResult SignOut()
		{
			_sessionSpecificDataProvider.RemoveCookie();
			_formsAuthentication.SignOut();

			var url = Request.Url;
			var issuerUrl = _authenticationModule.Issuer(_currentHttpContext.Current());

			var signInReply = new SignInRequestMessage(issuerUrl, _authenticationModule.Realm)
			{
				Context = "ru=" + url.AbsoluteUri.Remove(url.AbsoluteUri.IndexOf("Authentication/SignOut", StringComparison.OrdinalIgnoreCase)),
			};

			var signOut = new SignOutRequestMessage(issuerUrl, signInReply.WriteQueryString());

			return new RedirectResult(signOut.WriteQueryString());  
		}
	}

}