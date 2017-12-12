using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeTenantLogonInfoLoader : ITenantLogonInfoLoader
	{
		public IEnumerable<LogonInfo> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids)
		{
			return new List<LogonInfo>();
		}
	}
}