using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Tenant
{
	public class FakeAllTenantEtlSettings: IAllTenantEtlSettings
	{
		private readonly List<Infrastructure.MultiTenancy.Server.Tenant> _data = new List<Infrastructure.MultiTenancy.Server.Tenant>();

		public IEnumerable<TenantEtlSetting> All()
		{
			return _data.Select(x=>new TenantEtlSetting
			{
				Tenant = x.Name,
				RunIndexMaintenance = true,
				TimeZone = TimeZoneInfo.Utc
			});
		}

		public TenantEtlSetting Get(string tenant)
		{
			var single = _data.Single(x => x.Name == tenant);
			return new TenantEtlSetting
			{
				RunIndexMaintenance = true,
				Tenant = single.Name,
				TimeZone = TimeZoneInfo.Utc
			};
		}

		public void Has(Infrastructure.MultiTenancy.Server.Tenant tenant)
		{
			_data.Add(tenant);
		}

		public void Has(string tenant)
		{
			Has(new Infrastructure.MultiTenancy.Server.Tenant(tenant));
		}

		public void WasRemoved(string tenant)
		{
			var existing = _data.Single(x => x.Name == tenant);
			_data.Remove(existing);
		}
	}

	public class FakeTenants : 
		IFindTenantNameByRtaKey, 
		ICountTenants, 
		ILoadAllTenants, 
		IFindTenantByName,
		IAllTenantNames
	{
		private readonly List<Infrastructure.MultiTenancy.Server.Tenant> _data = new List<Infrastructure.MultiTenancy.Server.Tenant>();

		public void Has(Infrastructure.MultiTenancy.Server.Tenant tenant)
		{
			// making the key safe is done for real in db scripts when the default tenant is added
			var key = LegacyAuthenticationKey.MakeEncodingSafe(tenant.RtaKey);
			// when a test adds its own tenant with the legacy key, lets not add duplicates
			if (_data.Any(x => x.RtaKey == key))
				return;
			tenant.RtaKey = key;
			_data.Add(tenant);
		}
		
		public void Has(string tenant)
		{
			Has(new Infrastructure.MultiTenancy.Server.Tenant(tenant));
		}

		public void Has(string tenant, string rtaKey)
		{
			Has(new Infrastructure.MultiTenancy.Server.Tenant(tenant) {RtaKey = rtaKey});
		}

		public void WasRemoved(string tenant)
		{
			var existing = _data.Single(x => x.Name == tenant);
			_data.Remove(existing);
		}

		public string Find(string rtaKey)
		{
			var tenant = _data.SingleOrDefault(x => x.RtaKey == rtaKey);
			return tenant?.Name;
		}

		public int Count()
		{
			return _data.Count;
		}

		public IEnumerable<Infrastructure.MultiTenancy.Server.Tenant> Tenants()
		{
			return _data.ToArray();
		}

		Infrastructure.MultiTenancy.Server.Tenant IFindTenantByName.Find(string name)
		{
			return _data.Single(x => x.Name == name);
		}

		IEnumerable<string> IAllTenantNames.Tenants()
		{
			return _data.Select(x => x.Name).ToArray();
		}
	}
}