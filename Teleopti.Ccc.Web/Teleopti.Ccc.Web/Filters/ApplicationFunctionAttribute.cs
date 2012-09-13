using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Models.Shared;

namespace Teleopti.Ccc.Web.Filters
{
	public class ApplicationFunctionAttribute : AuthorizeAttribute
	{
		private readonly string[] _applicationFunctionPaths;

		public ApplicationFunctionAttribute() : this(null) { }

		public ApplicationFunctionAttribute(params string[] applicationFunctionPathses)
		{
			_applicationFunctionPaths = applicationFunctionPathses;
			Order = 3;
		}

		public IPermissionProvider PermissionProvider { get; set; }

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			if (_applicationFunctionPaths != null && _applicationFunctionPaths.Length>0)
			{
				var havePermission = false;
				foreach (var applicationFunctionPath in _applicationFunctionPaths)
				{
					if (PermissionProvider.HasApplicationFunctionPermission(applicationFunctionPath))
					{
						havePermission = true;
						break;
					}
				}
				
				if (!havePermission)
				{
					var isAjaxRequest = filterContext.HttpContext.Request.IsAjaxRequest();
					var viewResult = new ViewResult
					{
						ViewData = new ViewDataDictionary<ErrorViewModel>(new ErrorViewModel
						{
							Message = "No access"
						}),
						ViewName = isAjaxRequest ? "ErrorPartial" : "Error"
					};
					filterContext.Result = viewResult;
				}
			}
			base.OnAuthorization(filterContext);
		}

		protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
		{
			return true;
		}
	}
}