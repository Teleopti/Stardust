using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface ITenantLogonDataManagerClient
	{
		IEnumerable<LogonInfoModel> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids);
		LogonInfoModel GetLogonInfoForLogonName(string logonName);
		LogonInfoModel GetLogonInfoForIdentity(string identity);
		IEnumerable<LogonInfoModel> GetLogonInfoForIdentities(IEnumerable<string> identities);
	}
}