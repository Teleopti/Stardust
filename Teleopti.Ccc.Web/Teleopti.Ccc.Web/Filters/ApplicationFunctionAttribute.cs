using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Models.Shared;

namespace Teleopti.Ccc.Web.Filters
{
	public sealed class RequestPermissionAttribute : ApplicationFunctionAttribute
	{
		public RequestPermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.TextRequests, DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb)
		{
		}
	}

	public sealed class MyReportPermissionAttribute : ApplicationFunctionAttribute
	{
		public MyReportPermissionAttribute()
			:base(DefinedRaptorApplicationFunctionPaths.MyReportWeb)
		{
			
		}
	}

	public sealed class MyReportQueueMetricsPermissionAttribute : ApplicationFunctionAttribute
	{
		public MyReportQueueMetricsPermissionAttribute()
			:base(DefinedRaptorApplicationFunctionPaths.MyReportQueueMetrics)
		{
			
		}
	}

	public sealed class PreferencePermissionAttribute : ApplicationFunctionAttribute
	{
		public PreferencePermissionAttribute() : base(DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb,DefinedRaptorApplicationFunctionPaths.StandardPreferences)
		{
		}
	}

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