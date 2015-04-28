using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class ChangePasswordController : Controller
	{
		private readonly IChangePersonPassword _changePersonPassword;

		public ChangePasswordController(IChangePersonPassword changePersonPassword)
		{
			_changePersonPassword = changePersonPassword;
		}

		[TenantUnitOfWork]
		[HttpPost]
		public virtual void Modify(ChangePasswordModel model)
		{
			_changePersonPassword.Modify(model.UserName, model.OldPassword, model.NewPassword);
		}
	}
}