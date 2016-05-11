using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core;
using AuthorizationContext = System.Web.Mvc.AuthorizationContext;

namespace Teleopti.Ccc.Web.Filters
{
	// Simple way until we add conditional registration http://blogs.msdn.com/b/rickandy/archive/2011/05/02/securing-your-asp-net-mvc-3-application.aspx

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	
	public sealed class TeleoptiPrincipalAuthorizeAttribute : AuthorizeAttribute
	{
		private readonly IAuthenticationModule _authenticationModule;
		private readonly IIdentityProviderProvider _identityProviderProvider;
		private readonly IEnumerable<Type> _excludeControllerTypes;

		public string Realm { get; set; }

		public TeleoptiPrincipalAuthorizeAttribute(IAuthenticationModule authenticationModule, IIdentityProviderProvider identityProviderProvider, IEnumerable<Type> excludeControllerTypes = null)
		{
			_authenticationModule = authenticationModule;
			_identityProviderProvider = identityProviderProvider;
			Order = 2;
			_excludeControllerTypes = excludeControllerTypes ?? new List<Type>();
		}

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			var controllerType = filterContext.Controller.GetType();
			var controllerIsExcluded = _excludeControllerTypes.Any(t => t == controllerType || controllerType.IsSubclassOf(t));
			if (controllerIsExcluded)
				return;
			base.OnAuthorization(filterContext);
		}

		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			if (isInsideStartArea(httpContext))
				return httpContext.User.Identity.IsAuthenticated;
			return httpContext.User.Identity is ITeleoptiIdentity;
		}

		private static bool isInsideStartArea(HttpContextBase httpContext)
		{
			var areaToken = httpContext.Request.RequestContext.RouteData.DataTokens["area"];
			if (areaToken == null) return false;
			var area = areaToken.ToString();
			return area.ToUpperInvariant() == "START";
		}

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			if (filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
			{
				base.HandleUnauthorizedRequest(filterContext);
				return;
			}

			if (filterContext.HttpContext.User.Identity is IClaimsIdentity &&
			    filterContext.HttpContext.User.Identity.IsAuthenticated)
			{
				var targetArea = filterContext.RouteData.DataTokens["area"] ?? "Start";

				filterContext.Result = new RedirectToRouteResult(
					new RouteValueDictionary(
						new
							{
								controller = "Authentication",
								action = "",
								area = targetArea
							}
						)
					);
				return;
			}


			var signIn = new SignInRequestMessage(_authenticationModule.Issuer(filterContext.HttpContext), Realm ?? _authenticationModule.Realm)
			{
				Context = "ru=" + filterContext.HttpContext.Request.Path,
				HomeRealm = _identityProviderProvider.DefaultProvider()
			};

			var url = signIn.WriteQueryString();
			var uri = new Uri(url, UriKind.RelativeOrAbsolute);
			var redirectUrl = ConfigurationManager.AppSettings.ReadValue("UseRelativeConfiguration")
				? "/" + new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(uri)
				: url;

			filterContext.Result = new RedirectToRouteResult(
				new RouteValueDictionary(
					new
					{
						controller = "Return",
						action = "Hash",
						area = "Start",
						redirectUrl = redirectUrl
					}
					)
				);
		}
	}
}