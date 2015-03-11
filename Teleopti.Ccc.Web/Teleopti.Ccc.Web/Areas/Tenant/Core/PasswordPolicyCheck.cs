using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class PasswordPolicyCheck : IPasswordPolicyCheck
	{
		private readonly IConvertDataToOldUserDetailDomain _convertDataToOldUserDetailDomain;
		private readonly ICheckPasswordChange _checkPasswordChange;

		public PasswordPolicyCheck(IConvertDataToOldUserDetailDomain convertDataToOldUserDetailDomain, ICheckPasswordChange checkPasswordChange)
		{
			_convertDataToOldUserDetailDomain = convertDataToOldUserDetailDomain;
			_checkPasswordChange = checkPasswordChange;
		}

		public ApplicationAuthenticationResult Verify(PasswordPolicyForUser passwordPolicyForUser)
		{
			var userDetail = _convertDataToOldUserDetailDomain.Convert(passwordPolicyForUser);
			var res = _checkPasswordChange.Check(userDetail);
			if (res.Successful && res.Message == null)
			{
				return null;
			}
			return new ApplicationAuthenticationResult
			{
				FailReason = res.Message,
				PasswordExpired = res.PasswordExpired,
				Success = res.Successful,
				Tenant = passwordPolicyForUser.PersonInfo.Tenant
			};
		}
	}
}