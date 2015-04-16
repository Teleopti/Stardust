using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface IPasswordVerifier
	{
		bool Check(string userPassword, ApplicationLogonInfo applicationLogonInfo);
	}
}