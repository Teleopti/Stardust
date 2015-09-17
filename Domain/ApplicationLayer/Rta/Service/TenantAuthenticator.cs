namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class TenantAuthenticator : IAuthenticator
	{
		private readonly ITenantLoader _tenantLoader;

		public TenantAuthenticator(ITenantLoader tenantLoader)
		{
			_tenantLoader = tenantLoader;
		}

		public bool Authenticate(string authenticationKey)
		{
			return _tenantLoader.AuthenticateKey(authenticationKey);
		}
	}
}