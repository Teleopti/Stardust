using System;
using System.Linq;
using System.Web.Http.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.Web.Filters
{
	[CLSCompliant(false)]
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
	}
}