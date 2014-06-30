using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;

namespace Teleopti.Ccc.Web.Filters
{
	public sealed class AddFullDayAbsencePermissionAttribute : ApplicationFunctionAnywhereAttribute
	{
		public AddFullDayAbsencePermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.AddFullDayAbsence)
		{
		}
	}

	public sealed class AddIntradayAbsencePermissionAttribute : ApplicationFunctionAnywhereAttribute
	{
		public AddIntradayAbsencePermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.AddIntradayAbsence)
		{
		}
	}

	public sealed class RemoveAbsencePermissionAttribute : ApplicationFunctionAnywhereAttribute
	{
		public RemoveAbsencePermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.RemoveAbsence)
		{
		}
	}

	public sealed class AddActivityPermissionAttribute : ApplicationFunctionAnywhereAttribute
	{
		public AddActivityPermissionAttribute()
			: base(DefinedRaptorApplicationFunctionPaths.AddActivity)
		{
		}
	}

	public class ApplicationFunctionAnywhereAttribute : AuthorizeAttribute
	{
		private readonly string[] _applicationFunctionPaths;
		public IPermissionProvider PermissionProvider { get; set; }

		public ApplicationFunctionAnywhereAttribute() : this(null) { }

		public ApplicationFunctionAnywhereAttribute(params string[] applicationFunctionPathses)
		{
			_applicationFunctionPaths = applicationFunctionPathses;
			Order = 3;
		}

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			if (_applicationFunctionPaths != null && _applicationFunctionPaths.Length > 0)
			{
				var havePermission = _applicationFunctionPaths.Any(applicationFunctionPath => PermissionProvider.HasApplicationFunctionPermission(applicationFunctionPath));

				if (!havePermission)
				{
					filterContext.HttpContext.Response.StatusCode = 403;
					filterContext.Result = new JsonResult();
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