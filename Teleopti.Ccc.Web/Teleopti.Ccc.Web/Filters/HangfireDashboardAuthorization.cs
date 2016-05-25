using System.Collections.Generic;
using System.Linq;
using Hangfire.Dashboard;
using Microsoft.Owin;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Filters
{
	public class HangfireDashboardAuthorization : IAuthorizationFilter
	{
		public bool Authorize(IDictionary<string, object> owinEnvironment)
		{
			var context = new OwinContext(owinEnvironment);
			return isAuthenticated(context) && isSuperAdmin(context);
		}

		private static bool isAuthenticated(IOwinContext context)
		{
			return context.Authentication.User.Identity.IsAuthenticated;
		}

		private static bool isSuperAdmin(IOwinContext context)
		{
			var user = context.Authentication.User as IUnsafePerson;

			return user != null && user.Person.PermissionInformation.ApplicationRoleCollection.Any(x => x.Name == SystemUser.SuperRoleName);
		}
	}
}