using System.Collections.Generic;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public class BehaviorTestTenants : IAllTenantNames
	{
		public IEnumerable<string> Tenants()
		{
			yield return "TestData";
		}
	}
}