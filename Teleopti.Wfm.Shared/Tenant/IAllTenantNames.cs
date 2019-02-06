using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public interface IAllTenantNames
	{
		IEnumerable<string> Tenants();
	}
}