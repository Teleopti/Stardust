using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class SuccessfulPasswordPolicy : IVerifyPasswordPolicy
	{
		public PasswordPolicyResult Check(ApplicationLogonInfo userDetail)
		{
			return new PasswordPolicyResult{Successful = true};
		}
	}
}