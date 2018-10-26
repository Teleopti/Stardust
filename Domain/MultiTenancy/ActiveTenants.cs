using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public class ActiveTenants
	{
		private readonly IAllTenantNames _allTenantNames;
		private readonly object _lock = new object();
		private IEnumerable<string> _tenants = Enumerable.Empty<string>();

		public ActiveTenants(IAllTenantNames allTenantNames)
		{
			_allTenantNames = allTenantNames;
		}

		public void Update()
		{
			lock (_lock)
				_tenants = _allTenantNames.Tenants().ToArray();
		}

		public IEnumerable<string> Tenants() => _tenants;
	}
}