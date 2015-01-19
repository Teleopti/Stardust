using System;
using Teleopti.Ccc.Domain.Security.Authentication;

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

		public bool Verify(int invalidAttempts, DateTime? invalidAttemptsSequenceStart, DateTime? lastPasswordChange, out string passwordPolicyFailureReason)
		{
			var userDetail = _convertDataToOldUserDetailDomain.Convert(invalidAttempts, invalidAttemptsSequenceStart, lastPasswordChange);
			var res = _checkPasswordChange.Check(userDetail);
			passwordPolicyFailureReason = res.Message;
			return res.Successful;
		}
	}
}