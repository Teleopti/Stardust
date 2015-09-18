namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class TenantAuthenticator : IAuthenticator
	{
		private readonly TenantLoader _tenantLoader;

		public TenantAuthenticator(TenantLoader tenantLoader)
		{
			_tenantLoader = tenantLoader;
		}

		public bool Authenticate(string authenticationKey)
		{
			return _tenantLoader.AuthenticateKey(authenticationKey);
		}
	}
}