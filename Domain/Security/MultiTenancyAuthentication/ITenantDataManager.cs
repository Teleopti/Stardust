using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface ITenantDataManager
	{
		SavePersonInfoResult SaveTenantData(TenantAuthenticationData tenantAuthenticationData);
		void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted);
		
	}
}