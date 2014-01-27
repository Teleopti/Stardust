using System.Threading.Tasks;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Filters
{

	//MS: This needs to be AuthorizeAttribute because it needs to be executed before all other filters, and AuthorizeAttribute does just that.
	public sealed class CheckStartupResultAttribute : AuthorizeAttribute
	{
		private static object TaskWaitLockObject = new object();

		public CheckStartupResultAttribute()
		{
			Order = 0;
		}

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			if (ApplicationStartModule.TasksFromStartup != null)
			{
				lock (TaskWaitLockObject)
				{
					if (ApplicationStartModule.TasksFromStartup != null)
					{
						Task.WaitAll(ApplicationStartModule.TasksFromStartup);
						ApplicationStartModule.TasksFromStartup = null;
					}
				}
			}
			if (ApplicationStartModule.HasStartupError)
			{
                throw ApplicationStartModule.ErrorAtStartup;
			}
			base.OnAuthorization(filterContext);
		}

		protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
		{
			return true;
		}
	}
}