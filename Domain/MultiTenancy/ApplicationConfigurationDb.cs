using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public class ApplicationConfigurationDb
	{
		public Dictionary<ServerConfigurationKey, string> Server { get; set; }
		public Dictionary<TenantApplicationConfigKey, string> Tenant { get; set; }
	}
}
