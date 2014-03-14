﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.Web;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Filters
{
	// Simple way until we add conditional registration http://blogs.msdn.com/b/rickandy/archive/2011/05/02/securing-your-asp-net-mvc-3-application.aspx

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public sealed class TeleoptiPrincipalAuthorizeAttribute : AuthorizeAttribute
	{
		private readonly IAuthenticationModule _authenticationModule;
		private readonly IEnumerable<Type> _excludeControllerTypes;

		public string Realm { get; set; }

		public TeleoptiPrincipalAuthorizeAttribute(IAuthenticationModule authenticationModule)
			: this(authenticationModule, null)
		{
		}

		public TeleoptiPrincipalAuthorizeAttribute(IAuthenticationModule authenticationModule, IEnumerable<Type> excludeControllerTypes)
		{
			_authenticationModule = authenticationModule;
			Order = 2;
			_excludeControllerTypes = excludeControllerTypes ?? new List<Type>();
		}

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			if (!_excludeControllerTypes.Contains(filterContext.Controller.GetType()))
				base.OnAuthorization(filterContext);
		}

		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			return httpContext.User.Identity.IsAuthenticated;
		}

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
			{
				filterContext.Result = new HttpStatusCodeResult(403);
				return;
			}

			var signIn = new SignInRequestMessage(new Uri(_authenticationModule.Issuer), Realm ?? _authenticationModule.Realm)
			{
				Context = "ru=" + filterContext.HttpContext.Request.Path,
				HomeRealm = "urn:Teleopti"
			};

			filterContext.Result = new RedirectResult(signIn.WriteQueryString());
		}
	}
}