using System.Collections.Generic;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class BehaviorTestTenants : IAllTenantNames
	{
		public IEnumerable<string> Tenants()
		{
			yield return "TestData";
		}
	}
}