using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface ITenantDataManagerClient
	{
		SavePersonInfoResult SaveTenantData(TenantAuthenticationData tenantAuthenticationData);
		void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted);
		
	}
}