using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Fakes
{
	public class FindTenantByNameQueryFake : IFindTenantByNameQuery
	{
		private readonly IDictionary<string, Infrastructure.MultiTenancy.Server.Tenant> data;

		public FindTenantByNameQueryFake()
		{
			data = new Dictionary<string, Infrastructure.MultiTenancy.Server.Tenant>();
		}

		public Infrastructure.MultiTenancy.Server.Tenant Find(string name)
		{
			return data[name];
		}

		public void Add(Infrastructure.MultiTenancy.Server.Tenant tenant)
		{
			data[tenant.Name] = tenant;
		}
	}
}