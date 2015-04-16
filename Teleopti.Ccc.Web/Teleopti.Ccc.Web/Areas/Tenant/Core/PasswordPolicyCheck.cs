using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class PasswordPolicyCheck : IPasswordPolicyCheck
	{
		private readonly ITenantCheckPasswordChange _checkPasswordChange;

		public PasswordPolicyCheck(ITenantCheckPasswordChange checkPasswordChange)
		{
			_checkPasswordChange = checkPasswordChange;
		}

		public ApplicationAuthenticationResult Verify(ApplicationLogonInfo applicationLogonInfo)
		{
			var res = _checkPasswordChange.Check(applicationLogonInfo);
			if (res.Successful && res.Message == null)
			{
				return null;
			}
			return new ApplicationAuthenticationResult
			{
				FailReason = res.Message,
				PasswordExpired = res.PasswordExpired,
				Success = res.Successful,
				Tenant = applicationLogonInfo.PersonInfo.Tenant
			};
		}
	}
}