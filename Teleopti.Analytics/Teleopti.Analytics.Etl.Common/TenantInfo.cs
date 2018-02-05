using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Analytics.Etl.Common
{
	public class TenantInfo
	{
		public string Name => Tenant.Name;
		public Tenant Tenant { get; set; }
		public IDataSource DataSource { get; set; }
		public IBaseConfiguration EtlConfiguration { get; set; }
	}
}