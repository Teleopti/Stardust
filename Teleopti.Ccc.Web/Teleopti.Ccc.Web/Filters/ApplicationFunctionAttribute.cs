using System.Linq;
using System.Net;
using System.Net.Http;
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

		public ApplicationFunctionAttribute() : this((string[])null) { }

		//hack to make cls compliant
		public ApplicationFunctionAttribute(string applicationFunctionPath)
			: this(new[] { applicationFunctionPath}){}
		public ApplicationFunctionAttribute(string applicationFunctionPath1, string applicationFunctionPath2)
			: this(new[] { applicationFunctionPath1, applicationFunctionPath2 }) { }
		public ApplicationFunctionAttribute(string applicationFunctionPath1, string applicationFunctionPath2, string applicationFunctionPath3)
			: this(new[] { applicationFunctionPath1, applicationFunctionPath2, applicationFunctionPath3 }) { }


		private ApplicationFunctionAttribute(params string[] applicationFunctionPathses)
		{
			_applicationFunctionPaths = applicationFunctionPathses;
			Order = 3;
		}

		public IPermissionProvider PermissionProvider { get; set; }

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			const string noAccess = "No access";

			if (_applicationFunctionPaths == null || _applicationFunctionPaths.Length <= 0)
			{
				return;
			}

			var havePermission =
			 _applicationFunctionPaths.Any(
			  applicationFunctionPath => PermissionProvider.HasApplicationFunctionPermission(applicationFunctionPath));

			if (havePermission)
			{
				return;
			}

			if (filterContext.HttpContext.Request.IsAjaxRequest())
			{
				filterContext.Result = new JsonResult
				{
					Data = new
					{
						Message = noAccess
					},
					JsonRequestBehavior = JsonRequestBehavior.AllowGet
				};
			}
			else
			{
				var viewResult = new ViewResult
				{
					ViewData = new ViewDataDictionary<ErrorViewModel>(new ErrorViewModel
					{
						Message = noAccess
					}),
					ViewName = "Error"
				};
				filterContext.Result = viewResult;

				base.OnAuthorization(filterContext);
			}
		}

		protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
		{
			return true;
		}
	}
}