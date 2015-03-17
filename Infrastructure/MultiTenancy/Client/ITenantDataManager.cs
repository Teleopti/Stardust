using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface ITenantDataManager
	{
		void SaveTenantData(IEnumerable<TenantAuthenticationData> tenantAuthenticationData);
		void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted);
	}
}