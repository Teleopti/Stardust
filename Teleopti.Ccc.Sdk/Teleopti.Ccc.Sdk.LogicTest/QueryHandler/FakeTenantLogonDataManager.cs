using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	public class FakeTenantLogonDataManager : ITenantLogonDataManager
	{
		public IEnumerable<LogonInfoModel> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids)
		{
			return new LogonInfoModel[] {};
		}

		public LogonInfoModel GetLogonInfoForLogonName(string logonName)
		{
			throw new NotImplementedException();
		}

		public LogonInfoModel GetLogonInfoForIdentity(string identity)
		{
			throw new NotImplementedException();
		}
	}
}