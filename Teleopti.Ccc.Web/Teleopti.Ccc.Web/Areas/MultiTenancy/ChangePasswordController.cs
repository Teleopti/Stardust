using System.Web.Http;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class ChangePasswordController : ApiController
	{
		private readonly IChangePersonPassword _changePersonPassword;

		public ChangePasswordController(IChangePersonPassword changePersonPassword)
		{
			_changePersonPassword = changePersonPassword;
		}

		[TenantUnitOfWork]
		[HttpPost,Route("ChangePassword/Modify")]
		[NoTenantAuthentication]
		public virtual void Modify([FromBody]ChangePasswordModel model)
		{
			_changePersonPassword.Modify(model.PersonId, model.OldPassword, model.NewPassword);
		}
	}
}