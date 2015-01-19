using System;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public interface IPasswordPolicyCheck
	{
		bool Verify(int invalidAttempts, DateTime? invalidAttemptsSequenceStart, DateTime? lastPasswordChange, out string passwordPolicyFailureReason);
	}
}