using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Filters
{
	public class ApplicationFunctionApiAttribute : AuthorizeTeleoptiAttribute
	{
		private readonly string[] _applicationFunctionPaths;

		public ApplicationFunctionApiAttribute(params string[] applicationFunctionPaths) : base(null)
		{
			_applicationFunctionPaths = applicationFunctionPaths;
		}

		public ApplicationFunctionApiAttribute(Type[] excludeTypes, params string[] applicationFunctionPaths) : base(excludeTypes)
		{
			_applicationFunctionPaths = applicationFunctionPaths;
		}

		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			var isAuthorized = base.IsAuthorized(actionContext);

			if (isAuthorized && !_applicationFunctionPaths.IsNullOrEmpty())
			{
				var authorization = PrincipalAuthorization.Current_DONTUSE();
				isAuthorized =
					_applicationFunctionPaths.Any(
						applicationFunctionPath => authorization.IsPermitted(applicationFunctionPath));
			}
			return isAuthorized;
		}

		protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
		{
			if (actionContext == null)
			{
				throw new ArgumentNullException(nameof(actionContext));
			}
			actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, UserTexts.Resources.NotAllowed);
		}
	}
}