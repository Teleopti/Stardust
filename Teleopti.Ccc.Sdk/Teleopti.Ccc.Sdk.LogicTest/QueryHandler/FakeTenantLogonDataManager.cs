using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	public class FakeTenantLogonDataManager : ITenantLogonDataManager
	{
		private readonly IList<LogonInfoModel> storage = new List<LogonInfoModel>();

		public void SetLogon(Guid personGuid, string name, string identity)
		{
			storage.Add(new LogonInfoModel {PersonId = personGuid,LogonName = name,Identity = identity});
		}

		public IEnumerable<LogonInfoModel> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids)
		{
			return storage.Where(s => personGuids.Contains(s.PersonId));
		}

		public LogonInfoModel GetLogonInfoForLogonName(string logonName)
		{
			return storage.FirstOrDefault(s => s.LogonName == logonName);
		}

		public LogonInfoModel GetLogonInfoForIdentity(string identity)
		{
			return storage.FirstOrDefault(s => s.Identity == identity);
		}
	}
}