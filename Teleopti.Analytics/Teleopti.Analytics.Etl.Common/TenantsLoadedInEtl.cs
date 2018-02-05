using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Analytics.Etl.Common
{
	public class TenantsLoadedInEtl : IAllTenantNames
	{
		private readonly ITenants _tenants;

		public TenantsLoadedInEtl(ITenants tenants)
		{
			_tenants = tenants;
		}

		public IEnumerable<string> Tenants()
		{
			return _tenants
				.CurrentTenants()
				.Select(x => x.Name)
				.ToArray();
		}
	}
}