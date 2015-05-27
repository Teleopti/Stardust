using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface ITenantLogonInfoLoader
	{
		IEnumerable<LogonInfo> GetLogonInfoModelsForGuids(IEnumerable<Guid> personGuids);
	}
}