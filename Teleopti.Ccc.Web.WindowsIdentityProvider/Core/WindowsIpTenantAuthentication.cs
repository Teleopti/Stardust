using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
{
	public class WindowsIpTenantAuthentication : ITenantAuthentication
	{
		public bool Logon()
		{
			//put security here
			return true;
		}
	}
}