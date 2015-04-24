using System.Net;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class ChangePasswordController : Controller
	{
		private readonly IApplicationUserTenantQuery _applicationUserTenantQuery;

		public ChangePasswordController(IApplicationUserTenantQuery applicationUserTenantQuery)
		{
			_applicationUserTenantQuery = applicationUserTenantQuery;
		}

		public EmptyResult Modify(ChangePasswordModel model)
		{
			throw new HttpException(403, "Invalid username or password.");
		}
	}
}