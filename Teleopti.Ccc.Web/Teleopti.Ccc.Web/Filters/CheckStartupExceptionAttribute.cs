using System.Web.Mvc;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Filters
{

	//MS: This needs to be AuthorizeAttribute because it needs to be executed before everything else, and AuthorizeAttribute does just that.
	public class CheckStartupExceptionAttribute : AuthorizeAttribute
	{
		public CheckStartupExceptionAttribute()
		{
			Order = 0;
		}

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			if (ApplicationStartModule.HasStartupError)
				throw ApplicationStartModule.ErrorAtStartup;
			base.OnAuthorization(filterContext);
		}

		protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
		{
			return true;
		}
	}
}