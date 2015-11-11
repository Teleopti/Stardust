using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Filters
{
	public class AuthorizeTeleoptiAttribute : AuthorizeAttribute
	{
		private readonly IEnumerable<Type> _excludeControllerTypes;

		public AuthorizeTeleoptiAttribute(IEnumerable<Type> excludeControllerTypes)
		{
			_excludeControllerTypes = excludeControllerTypes ?? new List<Type>();
		}

		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			var controllerType = actionContext.ControllerContext.Controller.GetType();
			var controllerIsExcluded = _excludeControllerTypes.Any(t => t == controllerType || controllerType.IsSubclassOf(t));
			if (controllerIsExcluded)
				return true;
			return Thread.CurrentPrincipal is ITeleoptiPrincipal && base.IsAuthorized(actionContext);
		}
	}
}