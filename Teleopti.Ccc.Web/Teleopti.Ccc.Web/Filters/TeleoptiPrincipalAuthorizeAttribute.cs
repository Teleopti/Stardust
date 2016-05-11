using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Teleopti.Ccc.Domain.Security.Principal;
using AuthorizationContext = System.Web.Mvc.AuthorizationContext;

namespace Teleopti.Ccc.Web.Filters
{
	// Simple way until we add conditional registration http://blogs.msdn.com/b/rickandy/archive/2011/05/02/securing-your-asp-net-mvc-3-application.aspx

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	
	public sealed class TeleoptiPrincipalAuthorizeAttribute : AuthorizeAttribute
	{
		private readonly IEnumerable<Type> _excludeControllerTypes;

		public string Realm { get; set; }

		public TeleoptiPrincipalAuthorizeAttribute(IEnumerable<Type> excludeControllerTypes = null)
		{
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

			if (filterContext.HttpContext.User.Identity is ClaimsIdentity &&
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
			
			filterContext.Result = new HttpUnauthorizedResult();
		}
	}
}