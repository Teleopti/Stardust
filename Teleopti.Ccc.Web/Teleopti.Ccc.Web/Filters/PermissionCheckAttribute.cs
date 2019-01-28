using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Filters
{

	public class PermissionCheckAttribute : AuthorizeTeleoptiAttribute
	{
		private readonly string _functionPath;
		private readonly string _resourceName;

		public PermissionCheckAttribute(string functionPath, string resourceName) : base(new Type[] { })
		{
			_functionPath = functionPath;
			_resourceName = resourceName;
		}

		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			return base.IsAuthorized(actionContext) && PrincipalAuthorization.Current_DONTUSE().IsPermitted(_functionPath);
		}

		protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
		{
			if (actionContext == null)
			{
				throw new ArgumentNullException(nameof(actionContext));
			}
			actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, Resources.ResourceManager.GetString(_resourceName));
		}
	}
}