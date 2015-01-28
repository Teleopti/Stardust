using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface IPasswordVerifier
	{
		bool Check(string userPassword, PasswordPolicyForUser passwordPolicyForUser);
	}
}