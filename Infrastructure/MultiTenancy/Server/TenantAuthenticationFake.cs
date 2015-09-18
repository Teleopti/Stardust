namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class TenantAuthenticationFake : ITenantAuthentication
	{
		private bool _value = true;

		public bool Logon()
		{
			return _value;
		}

		public void NoAccess()
		{
			_value = false;
		}
	}
}