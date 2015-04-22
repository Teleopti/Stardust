using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface ITenantDataManager
	{
		void SaveTenantData(IEnumerable<TenantAuthenticationData> tenantAuthenticationData);
		SavePersonInfoResult SaveTenantData(TenantAuthenticationData tenantAuthenticationData);
		void DeleteTenantPersons(IEnumerable<Guid> personsToBeDeleted);
	}
}