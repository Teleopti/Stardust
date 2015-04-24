using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class ChangePasswordController : Controller
	{
		private readonly ChangePassword _changePassword;

		public ChangePasswordController(ChangePassword changePassword)
		{
			_changePassword = changePassword;
		}

		[TenantUnitOfWork]
		public virtual void Modify(ChangePasswordModel model)
		{
			_changePassword.Modify(model.UserName, model.OldPassword, model.NewPassword);
		}
	}
}