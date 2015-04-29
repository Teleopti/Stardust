using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface ITenantLogonDataManager
	{
		List<LogonInfoModel> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids);
	}
}