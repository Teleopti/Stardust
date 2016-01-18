using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public class CannotIterateAllTenants : IAllTenantNames
	{
		public IEnumerable<string> Tenants()
		{
			throw new NotImplementedException();
		}
	}
}