using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class FindTenantByNameQueryFake : IFindTenantByNameQuery
	{
		private readonly IDictionary<string, Tenant> data;

		public FindTenantByNameQueryFake()
		{
			data = new Dictionary<string, Tenant>();
		}

		public Tenant Find(string name)
		{
			return data[name];
		}

		public void Add(Tenant tenant)
		{
			data[tenant.Name] = tenant;
		}
	}
}