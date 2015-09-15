using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class FindTenantByNameWithEnsuredTransactionFake : IFindTenantByNameWithEnsuredTransaction
	{
		private readonly IDictionary<string, Tenant> tenants = new Dictionary<string, Tenant>();

		public Tenant Find(string name)
		{
			Tenant tenant;
			return tenants.TryGetValue(name, out tenant) ? tenant : null;
		}

		public void Has(Tenant tenant)
		{
			tenants[tenant.Name] = tenant;
		}
	}
}