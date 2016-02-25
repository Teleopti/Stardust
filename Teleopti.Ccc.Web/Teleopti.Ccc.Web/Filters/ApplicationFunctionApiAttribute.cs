using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.Web.Filters
{
	public class ApplicationFunctionApiAttribute : AuthorizeTeleoptiAttribute
	{
		private readonly string[] _applicationFunctionPaths;

		public ApplicationFunctionApiAttribute() : this(new string[] { }) { }

		private  ApplicationFunctionApiAttribute(params string[] applicationFunctionPaths) : base(null)
		{
			_applicationFunctionPaths = applicationFunctionPaths;
		}

		public ApplicationFunctionApiAttribute(string applicationFunctionPath) : this(new []{applicationFunctionPath})
		{
		}

		public ApplicationFunctionApiAttribute(string applicationFunctionPath1, string applicationFunctionPath2) : this(new []{applicationFunctionPath1,applicationFunctionPath2})
		{
		}

		public IPermissionProvider PermissionProvider { get; set; }

		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			var isAuthorized = base.IsAuthorized(actionContext);
			if (!isAuthorized)
			{
				return false;
			}
			
			if (_applicationFunctionPaths != null && _applicationFunctionPaths.Length > 0)
			{
				var havePermission =
					_applicationFunctionPaths.Any(
						applicationFunctionPath => PermissionProvider.HasApplicationFunctionPermission(applicationFunctionPath));
				if (!havePermission)
				{
					return false;
				}
			}
			return true;
		}

		protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
		{
			if (actionContext == null)
			{
				throw new ArgumentNullException("actionContext");
			}
			actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, UserTexts.Resources.NotAllowed);
		} 

	}
}