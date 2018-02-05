using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Analytics.Etl.Common
{
	public class AllTenantEtlSettings : IAllTenantEtlSettings
	{
		private readonly ITenants _tenants;

		public AllTenantEtlSettings(ITenants tenants)
		{
			_tenants = tenants;
		}

		public IEnumerable<TenantEtlSetting> All()
		{
			return _tenants.EtlTenants().Select(x=>new TenantEtlSetting
			{
				Tenant = x.Name,
				RunIndexMaintenance = x.EtlConfiguration.RunIndexMaintenance,
				TimeZone = TimeZoneInfo.FindSystemTimeZoneById(x.EtlConfiguration.TimeZoneCode)
			}).ToArray();
		}
	}
}