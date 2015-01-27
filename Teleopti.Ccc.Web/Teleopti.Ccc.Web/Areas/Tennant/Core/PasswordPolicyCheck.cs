using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
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

		public bool Verify(PasswordPolicyForUser passwordPolicyForUser, out string passwordPolicyFailureReason, out bool passwordExpired)
		{
			var userDetail = _convertDataToOldUserDetailDomain.Convert(passwordPolicyForUser);
			var res = _checkPasswordChange.Check(userDetail);
			passwordPolicyFailureReason = res.Message;
			passwordExpired = res.PasswordExpired;
			return res.Successful;
		}
	}
}