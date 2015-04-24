using System;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class ChangePasswordController : Controller
	{
		private readonly IApplicationUserTenantQuery _applicationUserTenantQuery;
		private readonly INow _now;
		private readonly IPasswordPolicy _passwordPolicy;

		public ChangePasswordController(IApplicationUserTenantQuery applicationUserTenantQuery,
																		INow now,
																		IPasswordPolicy passwordPolicy)
		{
			_applicationUserTenantQuery = applicationUserTenantQuery;
			_now = now;
			_passwordPolicy = passwordPolicy;
		}

		[TenantUnitOfWork]
		public EmptyResult Modify(ChangePasswordModel model)
		{
			var personInfo = _applicationUserTenantQuery.Find(model.UserName);
			if(personInfo==null || !personInfo.ApplicationLogonInfo.IsValidPassword(_now, _passwordPolicy, model.OldPassword))
				throw new HttpException(403, "Invalid username or password.");

			return null;
		}
	}
}