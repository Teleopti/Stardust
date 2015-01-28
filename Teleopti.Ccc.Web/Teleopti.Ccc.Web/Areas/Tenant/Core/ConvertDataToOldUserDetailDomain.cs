using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class ConvertDataToOldUserDetailDomain : IConvertDataToOldUserDetailDomain
	{
		public UserDetail Convert(PasswordPolicyForUser passwordPolicyForUser)
		{
			return new UserDetail(null)
			{
				InvalidAttempts = passwordPolicyForUser.InvalidAttempts,
				InvalidAttemptsSequenceStart = passwordPolicyForUser.InvalidAttemptsSequenceStart,
				LastPasswordChange = passwordPolicyForUser.LastPasswordChange
			};
		}
	}
}