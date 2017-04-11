namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class TenantAuthenticationAlwaysAuthenticated : ITenantAuthentication
	{

		public bool Logon()
		{
			return true;
		}
	}
}