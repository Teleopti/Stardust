using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public interface IAllTenantNames
	{
		IEnumerable<string> Tenants();
	}


	public interface IAllTenantEtlSettings
	{
		IEnumerable<TenantEtlSetting> All();
		TenantEtlSetting Get(string tenant);
	}

	public class TenantEtlSetting
	{
		public string Tenant { get; set; }
		public TimeZoneInfo TimeZone { get; set; }
		public bool RunIndexMaintenance { get; set; }
	}
}