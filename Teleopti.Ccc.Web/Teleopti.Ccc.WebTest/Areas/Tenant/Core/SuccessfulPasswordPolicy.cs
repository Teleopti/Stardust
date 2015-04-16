using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.Tenant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class SuccessfulPasswordPolicy : IPasswordPolicyCheck
	{
		public ApplicationAuthenticationResult Verify(ApplicationLogonInfo applicationLogonInfo)
		{
			return null;
		}
	}
}