using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
{
	public class CurrentTenantCredentialsFake : ICurrentTenantCredentials
	{
		private TenantCredentials _credentials;

		public TenantCredentials TenantCredentials()
		{
			return _credentials;
		}

		public void Has(TenantCredentials credentials)
		{
			_credentials = credentials;
		}
	}
}