using System.Linq;
using System.Web.Http.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.Web.Filters
{
	public class ApplicationFunctionApiAttribute : System.Web.Http.AuthorizeAttribute
	{
		private readonly string[] _applicationFunctionPaths;

		public ApplicationFunctionApiAttribute() : this(null) { }

		public ApplicationFunctionApiAttribute(params string[] applicationFunctionPaths)
		{
			_applicationFunctionPaths = applicationFunctionPaths;
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