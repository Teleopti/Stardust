using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Tenant
{
	public class FakeTenants : IFindTenantNameByRtaKey, ICountTenants, ILoadAllTenants
	{
		private readonly List<Infrastructure.MultiTenancy.Server.Tenant> _data = new List<Infrastructure.MultiTenancy.Server.Tenant>();

		public void Has(Infrastructure.MultiTenancy.Server.Tenant tenant)
		{
			// making the key safe is done for real in db scripts when the default tenant is added
			var key = ConfiguredKeyAuthenticator.MakeLegacyKeyEncodingSafe(tenant.RtaKey);
			// when a test adds its own tenant with the legacy key, lets not add duplicates
			if (_data.Any(x => x.RtaKey == key))
				return;
			tenant.RtaKey = key;
			_data.Add(tenant);
		}

		public string Find(string rtaKey)
		{
			var tenant = _data.SingleOrDefault(x => x.RtaKey == rtaKey);
			if (tenant == null)
				return null;
			return tenant.Name;
		}

		public int Count()
		{
			return _data.Count;
		}

		public IEnumerable<Infrastructure.MultiTenancy.Server.Tenant> Tenants()
		{
			return _data.ToArray();
		}
	}
}