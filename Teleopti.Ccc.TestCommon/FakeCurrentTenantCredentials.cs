using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentTenantCredentials : ICurrentTenantCredentials
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