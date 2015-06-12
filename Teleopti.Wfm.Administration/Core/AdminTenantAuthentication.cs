using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Wfm.Administration.Core
{
	public class AdminTenantAuthentication : ITenantAuthentication
	{
		public bool Logon()
		{
			//put security here
			return true;
		}
	}
}